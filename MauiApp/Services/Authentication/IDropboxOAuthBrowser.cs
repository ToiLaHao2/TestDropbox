namespace MauiApp.Services.Authentication;

internal interface IDropboxOAuthBrowser
{
	Uri RedirectUri { get; }
	Task<Uri> AuthenticateAsync(DropboxOAuthAttempt attempt, CancellationToken cancellationToken);
}

internal interface IDropboxSessionStore
{
	Task<string?> ReadAsync();
	Task WriteAsync(string session);
	Task RemoveAsync();
}
