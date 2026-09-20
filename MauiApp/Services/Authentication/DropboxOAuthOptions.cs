namespace MauiApp.Services.Authentication;

/// <summary>App key là định danh công khai, nhập trên UI; sample không dùng App secret.</summary>
internal static class DropboxOAuthOptions
{
	public const string WindowsRedirect = "http://127.0.0.1:52475/authorize";
	public const string AndroidScheme = "mauidropboxtutorial";
	public const string AndroidRedirect = AndroidScheme + "://oauth/callback";
	public const string AppKeyPreference = "Dropbox.OAuth.AppKey";
	public const string SessionStorageKey = "Dropbox.OAuth.Session.v1";
	public static readonly TimeSpan SignInTimeout = TimeSpan.FromMinutes(3);
	public static readonly string[] Scopes = ["files.metadata.read", "files.content.read"];
}
