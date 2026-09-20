using System.Net.Http;
using System.Text.Json;

namespace MauiApp.Services.Authentication;

/// <summary>Đổi code/refresh token qua HTTPS; không cần client_secret trong ứng dụng native.</summary>
internal sealed class DropboxOAuthProtocol
{
	private readonly Func<HttpClient> createClient;
	private readonly Func<DateTimeOffset> utcNow;

	public DropboxOAuthProtocol(Func<HttpClient>? createClient = null, Func<DateTimeOffset>? utcNow = null)
	{
		this.createClient = createClient ?? (() => new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
		{
			Timeout = TimeSpan.FromSeconds(30), MaxResponseContentBufferSize = 64 * 1024
		});
		this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
	}

	public Task<DropboxOAuthTokens> ExchangeAsync(DropboxOAuthAttempt attempt, Uri callback, CancellationToken cancellationToken) =>
		RequestAsync(attempt.AppKey, new Dictionary<string, string>
		{
			["grant_type"] = "authorization_code", ["client_id"] = attempt.AppKey,
			["code"] = attempt.ReadAuthorizationCode(callback), ["code_verifier"] = attempt.Verifier,
			["redirect_uri"] = attempt.RedirectUri.AbsoluteUri
		}, null, cancellationToken);

	public Task<DropboxOAuthTokens> RefreshAsync(DropboxOAuthTokens tokens, CancellationToken cancellationToken) =>
		RequestAsync(tokens.AppKey, new Dictionary<string, string>
		{
			["grant_type"] = "refresh_token", ["client_id"] = tokens.AppKey, ["refresh_token"] = tokens.RefreshToken
		}, tokens.RefreshToken, cancellationToken);

	private async Task<DropboxOAuthTokens> RequestAsync(string appKey, Dictionary<string, string> parameters,
		string? previousRefreshToken, CancellationToken cancellationToken)
	{
		try
		{
			using var client = createClient();
			using var body = new FormUrlEncodedContent(parameters);
			using var response = await client.PostAsync("https://api.dropboxapi.com/oauth2/token", body, cancellationToken).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				string? error = null;
				try
				{
					using var errorDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
					error = errorDocument.RootElement.TryGetProperty("error", out var errorValue) && errorValue.ValueKind == JsonValueKind.String
						? errorValue.GetString() : null;
				}
				catch (JsonException) { }
				if (error is "invalid_grant" or "invalid_client")
				{
					throw new DropboxAuthenticationException("Dropbox rejected the authorization or saved session. Verify App key and connect again.", true);
				}
				throw new DropboxAuthenticationException($"Dropbox token request failed (HTTP {(int)response.StatusCode}). Check App Console configuration or retry later.");
			}

			using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
			var root = document.RootElement;
			var accessToken = root.GetProperty("access_token").GetString();
			var refreshToken = root.TryGetProperty("refresh_token", out var refresh) ? refresh.GetString() : previousRefreshToken;
			var tokenType = root.GetProperty("token_type").GetString();
			if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken) ||
				!string.Equals(tokenType, "bearer", StringComparison.OrdinalIgnoreCase) ||
				!root.GetProperty("expires_in").TryGetInt32(out var lifetime) || lifetime <= 0)
			{
				throw new DropboxAuthenticationException("Dropbox returned an incomplete OAuth session. Reconnect with offline access enabled.");
			}
			if (root.TryGetProperty("scope", out var scope))
			{
				var granted = (scope.GetString() ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
				if (DropboxOAuthOptions.Scopes.Any(required => !granted.Contains(required, StringComparer.Ordinal)))
				{
					throw new DropboxAuthenticationException("Enable files.metadata.read and files.content.read in App Console, Submit, then connect again.");
				}
			}
			return new DropboxOAuthTokens
			{
				AppKey = appKey, AccessToken = accessToken, RefreshToken = refreshToken, ExpiresAt = utcNow().AddSeconds(lifetime)
			};
		}
		catch (OperationCanceledException) { throw; }
		catch (DropboxAuthenticationException) { throw; }
		catch (HttpRequestException)
		{
			throw new DropboxAuthenticationException("Could not reach the Dropbox token endpoint. Check the connection and retry.");
		}
		catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException or ArgumentException)
		{
			throw new DropboxAuthenticationException("Dropbox returned an invalid token response. No raw response was logged.");
		}
	}
}
