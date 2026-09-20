namespace MauiApp.Services.Authentication;

/// <summary>Ghép callback Android với đúng lần đăng nhập; bỏ callback lạ, lặp hoặc đến sau khi hủy.</summary>
internal static class OAuthCallbackRouter
{
	private static readonly object Sync = new();
	private static DropboxOAuthAttempt? activeAttempt;
	private static TaskCompletionSource<Uri>? pending;

	public static async Task<Uri> ReceiveAsync(DropboxOAuthAttempt attempt, Func<Uri, Task<bool>> openBrowser,
		CancellationToken cancellationToken)
	{
		var completion = new TaskCompletionSource<Uri>(TaskCreationOptions.RunContinuationsAsynchronously);
		lock (Sync)
		{
			if (pending is not null) { throw new DropboxAuthenticationException("A Dropbox sign-in is already in progress."); }
			activeAttempt = attempt;
			pending = completion;
		}
		try
		{
			using var registration = cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));
			cancellationToken.ThrowIfCancellationRequested();
			if (!await openBrowser(attempt.AuthorizeUri).ConfigureAwait(false))
			{
				throw new DropboxAuthenticationException("The system browser could not be opened.");
			}
			return await completion.Task.ConfigureAwait(false);
		}
		finally
		{
			lock (Sync)
			{
				if (ReferenceEquals(pending, completion)) { pending = null; activeAttempt = null; }
			}
		}
	}

	public static bool TryHandle(Uri callback)
	{
		lock (Sync)
		{
			return activeAttempt is not null && activeAttempt.MatchesCallback(callback) && pending!.TrySetResult(callback);
		}
	}
}
