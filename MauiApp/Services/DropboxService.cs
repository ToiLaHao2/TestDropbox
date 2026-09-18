using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using Dropbox.Api;
using Dropbox.Api.Files;
using MauiApp.Models;

namespace MauiApp.Services;

/// <summary>Kiểm tra kết nối, duyệt metadata và đọc file text nhỏ vào bộ nhớ.</summary>
public sealed class DropboxService
{
	public const int MaxTextFileBytes = 1024 * 1024;
	private readonly Func<HttpClient> createHttpClient;

	// Mỗi request dùng client riêng với timeout để giới hạn thời gian chờ của UI.
	public DropboxService() : this(() => new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
	{
	}

	// Cho phép dùng HTTP handler giả để kiểm tra mà không cần mạng hoặc token thật.
	internal DropboxService(Func<HttpClient> createHttpClient)
	{
		this.createHttpClient = createHttpClient;
	}

	/// <summary>Thử đọc một trang metadata nhỏ ở gốc, không thay đổi dữ liệu Dropbox.</summary>
	public async Task TestConnectionAsync(string accessToken)
	{
		await ReadFolderAsync(accessToken, string.Empty, null, 1).ConfigureAwait(false);
	}

	/// <summary>Đọc một trang file/folder và chuyển metadata SDK thành model hiển thị.</summary>
	/// <param name="accessToken">Token nhập trên UI; không lưu vào source hoặc cấu hình.</param>
	/// <param name="cursor">Null để bắt đầu; nếu có, dùng nguyên cursor của lần đọc trước.</param>
	/// <param name="folderPath">Path cho trang đầu; chuỗi rỗng là gốc. Có cursor thì không chọn lại thư mục bằng path.</param>
	public async Task<DropboxPage> ListFilesAsync(string accessToken, string? cursor = null, string folderPath = "")
	{
		var result = await ReadFolderAsync(accessToken, folderPath, cursor, 200).ConfigureAwait(false);
		// Không báo tải đủ khi phản hồi nói còn trang nhưng thiếu cursor để đọc tiếp.
		if (result.HasMore && string.IsNullOrWhiteSpace(result.Cursor))
		{
			throw new InvalidDataException("Dropbox returned more pages without a continuation cursor.");
		}

		// Folder không có kích thước file; null khác với file rỗng có Size = 0.
		var items = new List<DropboxItem>();
		foreach (var entry in result.Entries)
		{
			switch (entry)
			{
				case FileMetadata file:
					items.Add(new DropboxItem(file.Id, file.Name, file.PathDisplay ?? file.PathLower, false, file.Size));
					break;
				case FolderMetadata folder:
					items.Add(new DropboxItem(folder.Id, folder.Name, folder.PathDisplay ?? folder.PathLower, true, null));
					break;
			}
		}

		return new DropboxPage(items, result.HasMore ? result.Cursor : null);
	}

	/// <summary>Đọc file .txt UTF-8 tối đa 1 MiB; không ghi nội dung ra đĩa hoặc log.</summary>
	public async Task<DropboxTextPreview> ReadTextFileAsync(string accessToken, DropboxItem file,
		CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
		ArgumentNullException.ThrowIfNull(file);
		if (!file.CanReadText)
		{
			throw new NotSupportedException("Only .txt files can be previewed. Folders and other formats are not supported.");
		}
		ArgumentException.ThrowIfNullOrWhiteSpace(file.Id);
		if (file.Size > MaxTextFileBytes)
		{
			throw new InvalidDataException("Text preview is limited to 1 MiB (1,048,576 bytes).");
		}

		using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		timeout.CancelAfter(TimeSpan.FromSeconds(30));
		timeout.Token.ThrowIfCancellationRequested();
		using var httpClient = createHttpClient();
		using var registration = timeout.Token.Register(httpClient.CancelPendingRequests);
		var configuration = new DropboxClientConfig("MauiDropboxTutorial")
		{
			HttpClient = httpClient,
			MaxRetriesOnError = 0
		};

		try
		{
			using var client = new DropboxClient(accessToken.Trim(), configuration);
			// Dùng Id từ metadata để xác định file, không ghép đường dẫn cục bộ.
			using var download = await client.Files.DownloadAsync(file.Id).ConfigureAwait(false);
			timeout.Token.ThrowIfCancellationRequested();
			if (!download.Response.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
			{
				throw new NotSupportedException("The file is no longer a .txt file. Reload the folder and try again.");
			}
			if (download.Response.Size > MaxTextFileBytes)
			{
				throw new InvalidDataException("Text preview is limited to 1 MiB (1,048,576 bytes).");
			}

			using var source = await download.GetContentAsStreamAsync().ConfigureAwait(false);
			using var content = new MemoryStream();
			var buffer = new byte[8192];
			while (true)
			{
				// Đọc tối đa giới hạn + 1 byte để phát hiện vượt cỡ, kể cả khi metadata sai/cũ.
				var readLimit = Math.Min(buffer.Length, MaxTextFileBytes + 1 - (int)content.Length);
				var bytesRead = await source.ReadAsync(buffer.AsMemory(0, readLimit), timeout.Token).ConfigureAwait(false);
				if (bytesRead == 0)
				{
					break;
				}
				if (content.Length + bytesRead > MaxTextFileBytes)
				{
					throw new InvalidDataException("The downloaded content exceeds the 1 MiB text preview limit.");
				}
				content.Write(buffer, 0, bytesRead);
			}
			timeout.Token.ThrowIfCancellationRequested();
			if ((ulong)content.Length != download.Response.Size)
			{
				throw new IOException("The downloaded size does not match the file metadata. Reload and try again.");
			}

			var bytes = content.GetBuffer();
			var byteCount = (int)content.Length;
			var offset = byteCount >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;
			string text;
			try
			{
				text = new UTF8Encoding(false, true).GetString(bytes, offset, byteCount - offset);
			}
			catch (DecoderFallbackException)
			{
				// Không gắn inner exception: thông báo decoder có thể chứa byte của nội dung riêng tư.
				throw new InvalidDataException("The file is not valid UTF-8 text. Save it as UTF-8 and try again.");
			}
			if (text.Contains('\0'))
			{
				throw new InvalidDataException("The file contains NUL characters and is not supported as plain UTF-8 text.");
			}
			return new DropboxTextPreview(download.Response.Name, text, (ulong)byteCount);
		}
		catch (Exception exception)
		{
			Debug.WriteLine(DropboxDiagnostics.Create(exception, accessToken.Trim(), "/2/files/download"));
			throw;
		}
	}

	// Dùng chung việc kiểm tra đầu vào, chọn endpoint, quản lý client và ghi lỗi đã che bí mật.
	private async Task<ListFolderResult> ReadFolderAsync(string accessToken, string folderPath, string? cursor, uint limit)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
		ArgumentNullException.ThrowIfNull(folderPath);
		if (cursor is not null)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(cursor);
		}

		var operation = cursor is null ? "/2/files/list_folder" : "/2/files/list_folder/continue";
		using var httpClient = createHttpClient();
		var configuration = new DropboxClientConfig("MauiDropboxTutorial")
		{
			HttpClient = httpClient,
			MaxRetriesOnError = 0
		};

		try
		{
			// using giải phóng client; ConfigureAwait(false) không yêu cầu quay về UI thread.
			using var client = new DropboxClient(accessToken.Trim(), configuration);
			// Trang đầu chọn bằng path; trang sau tiếp tục bằng cursor, không trộn thư mục.
			return cursor is null
				? await client.Files.ListFolderAsync(folderPath, limit: limit).ConfigureAwait(false)
				: await client.Files.ListFolderContinueAsync(cursor).ConfigureAwait(false);
		}
		catch (Exception exception)
		{
			Debug.WriteLine(DropboxDiagnostics.Create(exception, accessToken.Trim(), operation, cursor));
			// Ném lại để UI xử lý, giữ nguyên stack trace của lỗi gốc.
			throw;
		}
	}
}
