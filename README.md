# MauiApp

.NET MAUI 10 application using XAML and C#, generated from Microsoft's template.
VS Code is configured for Windows. Android, iOS and Mac Catalyst targets remain
available in the project.

## Dropbox tutorial

The page uses `Dropbox.Api` 7.3.0 for a read-only connection check and paginated
file/folder listing with folder navigation. Enter your Dropbox **App key** and select
**Connect Dropbox** for Authorization Code + PKCE sign-in in the system browser.
No App secret or manually generated access token is required. After sign-in the root
loads automatically; click a folder or **Open folder** to browse inside.
**Up** opens the parent, **Root** returns to the accessible root, and **Load files**
reloads the current folder. **Load more** appends its next page if available.
Failed navigation keeps the previous folder and list; clearing/changing the token
resets the browser to root. **Test connection** remains a root metadata check.
**Read text** previews `.txt` UTF-8 files in memory with a 1 MiB download limit and
up to 32K characters displayed below the file list. This requires `files.content.read`.
No files are saved to disk; saving, import and upload remain unimplemented.
Do not put tokens or file contents in source code, terminal commands or logs.
OAuth sessions are stored in MAUI SecureStorage; expiring access tokens refresh automatically.
**Cancel sign-in** stops the current attempt (also limited to 3 minutes).
**Disconnect / Clear token** clears the local session, not Dropbox's browser login or authorization.
The optional manual-token field remains available for API-only testing and is not persisted.

### Configure OAuth first

In Dropbox App Console, enable and Submit `files.metadata.read` and `files.content.read`.
Register these exact redirect URIs under Settings / OAuth 2:

```text
http://127.0.0.1:52475/authorize
mauidropboxtutorial://oauth/callback
```

Windows uses a temporary IPv4 loopback listener bound only to `127.0.0.1`; Android uses
a callback activity, not an emulator HTTP listener. There is no index.html/JavaScript bridge.
Closing the browser tab alone does not cancel sign-in; return to the app and press Cancel.
Use the App key, **not** App secret, in the UI. Callback constants are in
`MauiApp/Services/Authentication/DropboxOAuthOptions.cs`. OAuth is configured only for
Windows and Android; Apple targets retain manual-token mode until callbacks are added.

Start with `MAUI_DROPBOX_TUTORIAL.md`, the concise Vietnamese guide for forum replies:
setup, connection, components, file operations, API sources and caveats.
`DROPBOX_TUTORIAL.md` remains the detailed implementation log and verification checklist.
On September 18, 2026, the user confirmed a successful connection after creating
a new Dropbox app. The previous app's scope/token issue remains deferred, not
resolved. Part 1's connection milestone is complete. Step 2.1 listing is implemented
and the user confirmed real folders are listed. Step 2.2 navigation is implemented
and the user confirmed folder browsing succeeds. Step 2.3 text preview is implemented
and builds with zero warnings/errors. In each of Debug and Release, 94 preview checks,
72 listing checks and 23 diagnostics checks pass using synthetic data/fake HTTP.
On September 20, OAuth was added: Windows and Android builds succeed with zero warnings/errors,
and 92 OAuth checks pass in each of Debug and Release, including real loopback socket callbacks
with fake authorization codes. Token exchange, refresh, browser and storage tests use fakes.
Real Dropbox OAuth consent, device SecureStorage, Android callback UI and text preview still
need user verification; no live credentials were used and no Android device was connected.
The tutorial keeps unverified test cases and file operations explicitly pending.
The page shows redacted diagnostic details, including exception messages, HTTP status
and Dropbox request IDs when available. Debug builds also log redacted details.
Review diagnostics for private data before sharing. Close any running app and
rebuild before testing changes; do not run an old binary with `--no-build`.

## Run in VS Code

1. Open the root folder `6006338` in VS Code (`code .`).
2. Trust the workspace only if you trust its source code.
3. Use Microsoft's C#, C# Dev Kit and .NET MAUI extensions (already installed).
4. Wait for `MauiApp.slnx` to load and NuGet restore to finish.
5. Open `MauiApp/MainPage.xaml.cs`. If prompted, select `MauiApp` as the C#
   startup project, `net10.0-windows10.0.19041.0` as the debug framework,
   and `Windows Machine` as the device in the C# status bar.
6. Select `MAUI - Windows` in Run and Debug, then press `F5`.

## Terminal commands

Run from the root folder:

```powershell
dotnet build MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0
dotnet run --project MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0
```

These commands limit restore/build to Windows so Android and Apple setup is
not needed for the initial build. The first restore requires access to NuGet.

## Main files

- `MauiApp/MainPage.xaml`: initial page UI.
- `MauiApp/MainPage.xaml.cs`: page event handlers.
- `MauiApp/Services/DropboxService.cs`: connection check and paginated metadata listing.
- `MauiApp/Services/Authentication/`: PKCE, callback receivers, secure sessions and refresh.
- `MauiApp/Platforms/Android/DropboxCallbackActivity.cs`: registered OAuth deep-link activity.
- `MauiApp/Models/DropboxItem.cs`: file/folder display data.
- `MauiApp/Models/DropboxPage.cs`: one page and its next cursor.
- `MauiApp/Models/DropboxTextPreview.cs`: decoded text held in memory, not a saved file.
- `MauiApp/AppShell.xaml`: navigation.
- `MauiApp/MauiProgram.cs`: app configuration and dependency injection.
- `MauiApp/Resources`: images, fonts and styles.

## Environment

- .NET SDK 10.0.4xx; `global.json` selects the latest installed stable patch.
- The `maui` workload and required VS Code extensions were already installed.
- Android additionally requires an Android SDK, JDK and emulator or device.
- Building iOS/Mac Catalyst requires macOS and a compatible Xcode installation.
