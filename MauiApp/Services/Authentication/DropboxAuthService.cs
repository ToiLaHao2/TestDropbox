using System.Text.Json;

namespace MauiApp.Services.Authentication;

/// <summary>Quản lý phiên OAuth riêng với chế độ token thủ công; tuần tự hóa refresh và sign-out.</summary>
internal sealed class DropboxAuthService(IDropboxOAuthBrowser browser, IDropboxSessionStore store,
	DropboxOAuthProtocol protocol, Func<DateTimeOffset>? utcNow = null)
{
	private readonly SemaphoreSlim gate = new(1, 1);
	private readonly Func<DateTimeOffset> utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
	private DropboxOAuthTokens? session;
	private volatile bool forceRefresh;
	public bool HasSession => session is not null;

	public async Task RestoreAsync(string appKey)
	{
		await gate.WaitAsync().ConfigureAwait(false);
		try
		{
			session = null;
			forceRefresh = false;
			try
			{
				var saved = await store.ReadAsync().ConfigureAwait(false);
				if (saved is null) { return; }
				var restored = JsonSerializer.Deserialize(saved, DropboxOAuthJsonContext.Default.DropboxOAuthTokens);
				if (restored is null || restored.AppKey != appKey || string.IsNullOrWhiteSpace(restored.AccessToken) ||
					string.IsNullOrWhiteSpace(restored.RefreshToken) || restored.ExpiresAt == default)
				{
					await store.RemoveAsync().ConfigureAwait(false);
					return;
				}
				session = restored;
			}
			catch (Exception)
			{
				session = null;
				throw new DropboxAuthenticationException("The saved session could not be read. Use Disconnect to clear it, then connect again.");
			}
		}
		finally { gate.Release(); }
	}

	public async Task SignInAsync(string appKey, CancellationToken cancellationToken)
	{
		DropboxOAuthAttempt.ValidateAppKey(appKey);
		using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		timeout.CancelAfter(DropboxOAuthOptions.SignInTimeout);
		await gate.WaitAsync(timeout.Token).ConfigureAwait(false);
		try
		{
			var attempt = new DropboxOAuthAttempt(appKey, browser.RedirectUri);
			var callback = await browser.AuthenticateAsync(attempt, timeout.Token).ConfigureAwait(false);
			var tokens = await protocol.ExchangeAsync(attempt, callback, timeout.Token).ConfigureAwait(false);
			timeout.Token.ThrowIfCancellationRequested();
			await PersistAsync(tokens).ConfigureAwait(false);
		}
		finally { gate.Release(); }
	}

	public async Task<string> GetAccessTokenAsync(string appKey, CancellationToken cancellationToken = default)
	{
		await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			if (session is null || session.AppKey != appKey)
			{
				throw new DropboxAuthenticationException("Connect Dropbox before loading files.", true);
			}
			if (!forceRefresh && session.ExpiresAt > utcNow().AddMinutes(1)) { return session.AccessToken; }
			try
			{
				var refreshed = await protocol.RefreshAsync(session, cancellationToken).ConfigureAwait(false);
				cancellationToken.ThrowIfCancellationRequested();
				await PersistAsync(refreshed).ConfigureAwait(false);
				return refreshed.AccessToken;
			}
			catch (DropboxAuthenticationException exception) when (exception.RequiresSignIn)
			{
				session = null;
				await RemoveSavedSessionAsync().ConfigureAwait(false);
				throw;
			}
		}
		finally { gate.Release(); }
	}

	// API có thể từ chối token trước hạn: lần thao tác kế tiếp thử refresh, không lặp request vô hạn.
	public void MarkAccessTokenExpired() => forceRefresh = true;

	public async Task SignOutAsync()
	{
		await gate.WaitAsync().ConfigureAwait(false);
		try
		{
			session = null;
			forceRefresh = false;
			await RemoveSavedSessionAsync().ConfigureAwait(false);
		}
		finally { gate.Release(); }
	}

	private async Task PersistAsync(DropboxOAuthTokens tokens)
	{
		try { await store.WriteAsync(JsonSerializer.Serialize(tokens, DropboxOAuthJsonContext.Default.DropboxOAuthTokens)).ConfigureAwait(false); }
		catch (Exception)
		{
			throw new DropboxAuthenticationException("The OAuth session could not be saved securely. Check device storage and reconnect.");
		}
		session = tokens;
		forceRefresh = false;
	}

	private async Task RemoveSavedSessionAsync()
	{
		try { await store.RemoveAsync().ConfigureAwait(false); }
		catch (Exception)
		{
			throw new DropboxAuthenticationException("The saved session could not be removed. Retry Disconnect before leaving this device.");
		}
	}
}
