namespace MauiApp.Services.Authentication;

/// <summary>Chỉ serialize vào SecureStorage. Không dùng record để tránh ToString tự in token.</summary>
internal sealed class DropboxOAuthTokens
{
	public string AppKey { get; init; } = string.Empty;
	public string AccessToken { get; init; } = string.Empty;
	public string RefreshToken { get; init; } = string.Empty;
	public DateTimeOffset ExpiresAt { get; init; }
}
