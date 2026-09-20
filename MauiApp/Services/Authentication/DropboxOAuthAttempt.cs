using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MauiApp.Services.Authentication;

/// <summary>Một lần đăng nhập có state và PKCE verifier riêng; không lưu hoặc log URL OAuth.</summary>
internal sealed class DropboxOAuthAttempt
{
	internal string Verifier { get; } = Base64Url(RandomNumberGenerator.GetBytes(32));
	private readonly string state = Base64Url(RandomNumberGenerator.GetBytes(32));
	public string AppKey { get; }
	public Uri RedirectUri { get; }
	public Uri AuthorizeUri { get; }

	public DropboxOAuthAttempt(string appKey, Uri redirectUri)
	{
		ValidateAppKey(appKey);
		if (!redirectUri.IsAbsoluteUri || redirectUri.Query.Length != 0 || redirectUri.Fragment.Length != 0 ||
			!(redirectUri.AbsoluteUri == DropboxOAuthOptions.AndroidRedirect ||
			  (redirectUri.Scheme == "http" && redirectUri.Host == "127.0.0.1" && redirectUri.AbsolutePath == "/authorize")))
		{
			throw new DropboxAuthenticationException("The OAuth callback configuration is invalid.");
		}
		AppKey = appKey;
		RedirectUri = redirectUri;
		var parameters = new Dictionary<string, string>
		{
			["client_id"] = appKey,
			["response_type"] = "code",
			["redirect_uri"] = redirectUri.AbsoluteUri,
			["state"] = state,
			["code_challenge"] = CreateChallenge(Verifier),
			["code_challenge_method"] = "S256",
			["token_access_type"] = "offline",
			["scope"] = string.Join(" ", DropboxOAuthOptions.Scopes)
		};
		AuthorizeUri = new Uri("https://www.dropbox.com/oauth2/authorize?" + string.Join("&",
			parameters.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}")));
	}

	public static void ValidateAppKey(string appKey)
	{
		if (string.IsNullOrWhiteSpace(appKey) ||
			!Regex.IsMatch(appKey, @"\A[A-Za-z0-9_-]{3,128}\z", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)))
		{
			throw new DropboxAuthenticationException("Enter the App key from Dropbox App Console, not an access token or App secret.");
		}
	}

	// Callback sai state/endpoint không được hoàn tất phiên đang chờ, kể cả callback đến muộn.
	public bool MatchesCallback(Uri callback)
	{
		if (!callback.IsAbsoluteUri || callback.OriginalString.Length > 8192 || callback.Fragment.Length != 0 ||
			callback.Scheme != RedirectUri.Scheme || callback.Host != RedirectUri.Host ||
			callback.Port != RedirectUri.Port || callback.AbsolutePath != RedirectUri.AbsolutePath || callback.UserInfo.Length != 0)
		{
			return false;
		}
		try
		{
			var parameters = ParseQuery(callback);
			return parameters.TryGetValue("state", out var returnedState) &&
				CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(state), Encoding.UTF8.GetBytes(returnedState));
		}
		catch (DropboxAuthenticationException)
		{
			return false;
		}
	}

	public string ReadAuthorizationCode(Uri callback)
	{
		if (!MatchesCallback(callback))
		{
			throw new DropboxAuthenticationException("OAuth callback validation failed. Start Connect Dropbox again.");
		}
		var parameters = ParseQuery(callback);
		if (parameters.TryGetValue("error", out var error))
		{
			throw new DropboxAuthenticationException(error == "access_denied"
				? "Dropbox permission was declined. No session was saved."
				: "Dropbox did not authorize this sign-in. Check App key, redirect URIs and Permissions.");
		}
		if (!parameters.TryGetValue("code", out var code) || string.IsNullOrWhiteSpace(code) || code.Length > 4096)
		{
			throw new DropboxAuthenticationException("The callback did not contain a valid authorization code. Connect again.");
		}
		return code;
	}

	internal static Dictionary<string, string> ParseQuery(Uri uri)
	{
		var result = new Dictionary<string, string>(StringComparer.Ordinal);
		foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
		{
			var separator = pair.IndexOf('=');
			var key = WebUtility.UrlDecode(separator < 0 ? pair : pair[..separator]);
			var value = separator < 0 ? string.Empty : WebUtility.UrlDecode(pair[(separator + 1)..]);
			if (!result.TryAdd(key, value))
			{
				throw new DropboxAuthenticationException("The OAuth callback contains duplicate parameters.");
			}
		}
		return result;
	}

	internal static string CreateChallenge(string verifier) => Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
	private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
