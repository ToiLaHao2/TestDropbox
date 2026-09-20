namespace MauiApp.Services.Authentication;

/// <summary>Chỉ chứa thông báo an toàn; không gắn exception gốc có thể chứa code/token.</summary>
internal sealed class DropboxAuthenticationException(string message, bool requiresSignIn = false) : Exception(message)
{
	public bool RequiresSignIn { get; } = requiresSignIn;
}
