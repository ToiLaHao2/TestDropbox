using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dropbox.Api;

namespace MauiApp.Services;

/// <summary>Tạo chẩn đoán; che bí mật đã biết trước khi hiển thị hoặc ghi Debug.</summary>
internal static class DropboxDiagnostics
{
	// Dùng scope thật từ lỗi có cấu trúc; không gán lỗi quyền đọc nội dung thành metadata.
	public static bool IsMissingMetadataScope(Exception exception) =>
		string.Equals(GetMissingScope(exception), "files.metadata.read", StringComparison.OrdinalIgnoreCase);

	public static string? GetMissingScope(Exception exception) => exception switch
	{
		AuthException authException => authException.ErrorResponse?.AsMissingScope?.Value.RequiredScope,
		BadInputException badInputException => badInputException.Message.Contains(
			"does not have the required scope 'files.metadata.read'", StringComparison.OrdinalIgnoreCase)
			? "files.metadata.read"
			: badInputException.Message.Contains("does not have the required scope 'files.content.read'", StringComparison.OrdinalIgnoreCase)
				? "files.content.read" : null,
		_ => null
	};

	/// <summary>Tổng hợp endpoint, exception, HTTP status nếu có và request ID để đối chiếu lỗi.</summary>
	public static string Create(Exception exception, string accessToken,
		string operation = "/2/files/list_folder", string? cursor = null)
	{
		var details = new StringBuilder();
		details.AppendLine($"UTC: {DateTimeOffset.UtcNow:O}");
		var category = operation switch
		{
			"/2/files/list_folder" or "/2/files/list_folder/continue" => "metadata only",
			"/2/files/download" => "file content",
			_ => "Dropbox API"
		};
		details.AppendLine($"Operation: {operation} ({category})");
		details.AppendLine("SDK: Dropbox.Api 7.3.0");

		// Giới hạn chuỗi lỗi lồng nhau; không suy đoán HTTP status khi exception không cung cấp.
		Exception? currentException = exception;
		for (var depth = 0; currentException is not null && depth < 5; depth++)
		{
			details.AppendLine();
			details.AppendLine($"{(depth == 0 ? "Exception" : "Inner exception")}: {currentException.GetType().FullName}");
			int? statusCode = currentException switch
			{
				HttpException httpException => httpException.StatusCode,
				HttpRequestException requestException => (int?)requestException.StatusCode,
				_ => null
			};
			details.AppendLine($"HTTP status: {statusCode?.ToString() ?? "Not exposed by this exception"}");

			if (currentException is DropboxException dropboxException)
			{
				details.AppendLine($"Dropbox request ID: {dropboxException.RequestId ?? "Not available"}");
			}

			details.AppendLine($"Message: {currentException.Message}");
			// Stack trace chỉ có trong Debug và vẫn được che bí mật trước khi xuất chẩn đoán.
#if DEBUG
			if (!string.IsNullOrWhiteSpace(currentException.StackTrace))
			{
				details.AppendLine($"Stack trace: {currentException.StackTrace}");
			}
#endif
			currentException = currentException.InnerException;
		}

		return Redact(details.ToString(), accessToken, cursor);
	}

	// Che bí mật đã biết và trường nhạy cảm phổ biến; vẫn cần kiểm tra riêng tư thủ công.
	private static string Redact(string details, params string?[] credentials)
	{
		foreach (var credential in credentials)
		{
			if (string.IsNullOrEmpty(credential))
			{
				continue;
			}
			// Token/cursor có thể xuất hiện nguyên văn, URL-encoded hoặc JSON-escaped trong lỗi.
			var encodedCredential = JsonSerializer.Serialize(credential);
			foreach (var secret in new[] { credential, Uri.EscapeDataString(credential), encodedCredential[1..^1] })
			{
				details = details.Replace(secret, "[REDACTED]", StringComparison.Ordinal);
			}
		}

		try
		{
			details = Regex.Replace(details,
				@"\bBearer\s+[^\s""',;]+",
				"Bearer [REDACTED]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
			details = Regex.Replace(details,
				@"(\b(?:access_token|refresh_token|client_secret|app_secret|authorization|cursor)\b[""']?\s*[:=]\s*)(?:""[^""]*""|'[^']*'|[^\s,;&}]+)",
				"$1[REDACTED]", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
		}
		// Nếu che bí mật quá thời gian giới hạn, bỏ chẩn đoán thay vì trả lỗi thô.
		catch (RegexMatchTimeoutException)
		{
			return "Diagnostics omitted because credential redaction timed out. No raw error was logged.";
		}

		// Cắt ngắn sau khi che bí mật; tên/đường dẫn file riêng tư vẫn cần xem lại trước khi chia sẻ.
		return details.Length <= 8000 ? details : details[..8000] + "\n[Diagnostics truncated]";
	}
}
