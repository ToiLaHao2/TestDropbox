namespace MauiApp.Services.Authentication;

internal static class MauiDropboxOAuth
{
	public static DropboxAuthService CreateService() => new(new PlatformBrowser(), new SessionStore(), new DropboxOAuthProtocol());

	private sealed class SessionStore : IDropboxSessionStore
	{
		public Task<string?> ReadAsync() => SecureStorage.Default.GetAsync(DropboxOAuthOptions.SessionStorageKey);
		public Task WriteAsync(string session) => SecureStorage.Default.SetAsync(DropboxOAuthOptions.SessionStorageKey, session);
		public Task RemoveAsync()
		{
			SecureStorage.Default.Remove(DropboxOAuthOptions.SessionStorageKey);
			return Task.CompletedTask;
		}
	}

	private sealed class PlatformBrowser : IDropboxOAuthBrowser
	{
		public Uri RedirectUri =>
#if WINDOWS
			new(DropboxOAuthOptions.WindowsRedirect);
#elif ANDROID
			new(DropboxOAuthOptions.AndroidRedirect);
#else
			throw new DropboxAuthenticationException("OAuth is configured only for Windows and Android in this sample.");
#endif

		public Task<Uri> AuthenticateAsync(DropboxOAuthAttempt attempt, CancellationToken cancellationToken)
		{
#if WINDOWS
			return LoopbackOAuthReceiver.ReceiveAsync(attempt, OpenBrowserAsync, cancellationToken);
#elif ANDROID
			return OAuthCallbackRouter.ReceiveAsync(attempt, OpenBrowserAsync, cancellationToken);
#else
			throw new DropboxAuthenticationException("OAuth is configured only for Windows and Android in this sample.");
#endif
		}

		private static Task<bool> OpenBrowserAsync(Uri uri) => MainThread.InvokeOnMainThreadAsync(
			() => Browser.Default.OpenAsync(uri, BrowserLaunchMode.External));
	}
}
