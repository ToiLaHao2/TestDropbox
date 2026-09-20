using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MauiApp.Services.Authentication;

/// <summary>Callback HTTP chỉ bind 127.0.0.1; TcpListener không yêu cầu URL ACL của HttpListener.</summary>
internal static class LoopbackOAuthReceiver
{
	public static async Task<Uri> ReceiveAsync(DropboxOAuthAttempt attempt, Func<Uri, Task<bool>> openBrowser,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (attempt.RedirectUri.Scheme != "http" || attempt.RedirectUri.Host != "127.0.0.1")
		{
			throw new DropboxAuthenticationException("The Windows callback must use the configured IPv4 loopback address.");
		}
		using var listener = new TcpListener(IPAddress.Loopback, attempt.RedirectUri.Port) { ExclusiveAddressUse = true };
		try { listener.Start(4); }
		catch (SocketException)
		{
			throw new DropboxAuthenticationException($"Cannot listen on port {attempt.RedirectUri.Port}. Close another running sample, or update both the callback constant and App Console redirect URI.");
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (!await openBrowser(attempt.AuthorizeUri).ConfigureAwait(false))
		{
			throw new DropboxAuthenticationException("The system browser could not be opened.");
		}
		while (true)
		{
			using var connection = await listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
			using var requestTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			requestTimeout.CancelAfter(TimeSpan.FromSeconds(5));
			var stream = connection.GetStream();
			Uri? callback = null;
			try
			{
				var buffer = new byte[8192];
				var received = 0;
				while (received < buffer.Length)
				{
					var count = await stream.ReadAsync(buffer.AsMemory(received), requestTimeout.Token).ConfigureAwait(false);
					if (count == 0) { break; }
					received += count;
					if (Encoding.ASCII.GetString(buffer, 0, received).Contains("\r\n\r\n", StringComparison.Ordinal)) { break; }
				}
				var request = Encoding.ASCII.GetString(buffer, 0, received);
				var firstLine = request.Split("\r\n", 2)[0].Split(' ');
				if (request.Contains("\r\n\r\n", StringComparison.Ordinal) && firstLine.Length == 3 &&
					firstLine[0] == "GET" && (firstLine[2] is "HTTP/1.1" or "HTTP/1.0") &&
					firstLine[1].StartsWith('/') && !firstLine[1].StartsWith("//", StringComparison.Ordinal) &&
					Uri.TryCreate(attempt.RedirectUri, firstLine[1], out var candidate) && attempt.MatchesCallback(candidate))
				{
					callback = candidate;
				}
				var message = callback is null ? "Invalid callback. Return to the app or continue signing in." :
					"Dropbox callback received. Return to the app to see the sign-in result. You can close this tab.";
				var body = Encoding.UTF8.GetBytes("<!doctype html><html><body>" + message + "</body></html>");
				var headers = Encoding.ASCII.GetBytes($"HTTP/1.1 {(callback is null ? "400 Bad Request" : "200 OK")}\r\n" +
					$"Content-Type: text/html; charset=utf-8\r\nContent-Length: {body.Length}\r\n" +
					"Cache-Control: no-store\r\nReferrer-Policy: no-referrer\r\nContent-Security-Policy: default-src 'none'\r\nConnection: close\r\n\r\n");
				await stream.WriteAsync(headers, requestTimeout.Token).ConfigureAwait(false);
				await stream.WriteAsync(body, requestTimeout.Token).ConfigureAwait(false);
			}
			catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { }
			catch (IOException) { }
			cancellationToken.ThrowIfCancellationRequested();
			if (callback is not null) { return callback; }
		}
	}
}
