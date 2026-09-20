using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using MauiApp.Services.Authentication;

namespace MauiApp;

// Callback native, không mở HTTP server bên trong emulator và không cần index.html.
[Activity(Exported = true, NoHistory = true, LaunchMode = LaunchMode.SingleTop,
	Theme = "@android:style/Theme.Translucent.NoTitleBar")]
[IntentFilter([Intent.ActionView], Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
	DataScheme = DropboxOAuthOptions.AndroidScheme, DataHost = "oauth", DataPath = "/callback")]
public sealed class DropboxCallbackActivity : Activity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		HandleCallback(Intent);
	}

	protected override void OnNewIntent(Intent? intent)
	{
		base.OnNewIntent(intent);
		HandleCallback(intent);
	}

	private void HandleCallback(Intent? intent)
	{
		if (Uri.TryCreate(intent?.DataString, UriKind.Absolute, out var callback) && OAuthCallbackRouter.TryHandle(callback))
		{
			var returnToApp = new Intent(this, typeof(MainActivity));
			returnToApp.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
			StartActivity(returnToApp);
		}
		Finish();
	}
}
