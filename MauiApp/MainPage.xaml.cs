using System.Collections.ObjectModel;
using System.Net.Http;
using Dropbox.Api;
using Dropbox.Api.Files;
using MauiApp.Models;
using MauiApp.Services;
using MauiApp.Services.Authentication;

namespace MauiApp;

/// <summary>Điều phối UI kết nối, duyệt thư mục, phân trang và hiển thị lỗi của bản demo.</summary>
public partial class MainPage : ContentPage
{
	private readonly DropboxService dropboxService = new();
	private readonly DropboxAuthService authentication = MauiDropboxOAuth.CreateService();
	private CancellationTokenSource? signInCancellation;
	private bool restoreAttempted;
	private readonly ObservableCollection<DropboxItem> items = new();
	// Id -> vị trí trong collection, giúp cập nhật mục đã có thay vì thêm bản sao khi nối trang.
	private readonly Dictionary<string, int> itemIndexes = new(StringComparer.Ordinal);
	// Cursor thuộc lượt liệt kê hiện tại; đường dẫn rỗng biểu diễn thư mục gốc.
	private string? nextCursor;
	private string currentFolderPath = string.Empty;
	private bool isBusy;

	// Nhãn rỗng ở Footer tránh cấu trúc EmptyView từng nghi chặn chuột.
	public MainPage()
	{
		InitializeComponent();
		items.CollectionChanged += (sender, eventArgs) => EmptyListLabel.IsVisible = items.Count == 0;
		FilesCollectionView.ItemsSource = items;
		AppKeyEntry.Text = Preferences.Default.Get(DropboxOAuthOptions.AppKeyPreference, string.Empty);
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (restoreAttempted || isBusy) { return; }
		restoreAttempted = true;
		SetBusy(true);
		try
		{
			await authentication.RestoreAsync(AppKeyEntry.Text?.Trim() ?? string.Empty);
			if (authentication.HasSession) { StatusLabel.Text = "Saved OAuth session restored. Select Load files."; }
		}
		catch (Exception exception) { ShowSignInFailure(exception); }
		finally { SetBusy(false); }
	}

	private async void OnConnectDropboxClicked(object? sender, EventArgs eventArgs)
	{
		if (isBusy) { return; }
		ClearDiagnostics();
		ResetBrowser();
		using var cancellation = new CancellationTokenSource();
		signInCancellation = cancellation;
		SetBusy(true);
		AccessTokenEntry.Text = string.Empty;
		CancelSignInButton.IsVisible = true;
		CancelSignInButton.IsEnabled = true;
		var connected = false;
		try
		{
			var appKey = AppKeyEntry.Text?.Trim() ?? string.Empty;
			DropboxOAuthAttempt.ValidateAppKey(appKey);
			Preferences.Default.Set(DropboxOAuthOptions.AppKeyPreference, appKey);
			StatusLabel.Text = "Complete Dropbox sign-in in the browser, then return here. Cancel or wait up to 3 minutes.";
			await authentication.SignInAsync(appKey, cancellation.Token);
			connected = true;
			StatusLabel.Text = "Signed in. Loading your Dropbox root...";
		}
		catch (Exception exception) { ShowSignInFailure(exception); }
		finally
		{
			signInCancellation = null;
			CancelSignInButton.IsVisible = false;
			SetBusy(false);
		}
		if (connected) { await LoadPageAsync(false, string.Empty); }
	}

	private void OnCancelSignInClicked(object? sender, EventArgs eventArgs)
	{
		signInCancellation?.Cancel();
		CancelSignInButton.IsEnabled = false;
	}

	// Không hiển thị exception thô của trình duyệt/token endpoint vì có thể chứa bí mật.
	private void ShowSignInFailure(Exception exception)
	{
		StatusLabel.Text = exception switch
		{
			DropboxAuthenticationException failure => failure.Message,
			OperationCanceledException => "Sign-in canceled or timed out. Return to the app and select Connect Dropbox to retry.",
			_ => "Sign-in could not be completed. Check the browser, App Console callback settings and secure storage, then retry."
		};
	}

	private async Task<string> GetRequestAccessTokenAsync()
	{
		if (authentication.HasSession)
		{
			return await authentication.GetAccessTokenAsync(AppKeyEntry.Text?.Trim() ?? string.Empty);
		}
		var token = AccessTokenEntry.Text?.Trim();
		if (string.IsNullOrWhiteSpace(token))
		{
			throw new DropboxAuthenticationException("Connect Dropbox, or enter a manual token for testing. No file request was sent.");
		}
		return token;
	}

	// Kiểm tra đọc ở gốc mà không đổi thư mục hoặc danh sách đang xem.
	private async void OnTestConnectionClicked(object? sender, EventArgs eventArgs)
	{
		if (isBusy)
		{
			return;
		}

		ClearDiagnostics();
		var accessToken = string.Empty;

		SetBusy(true);
		StatusLabel.Text = "Checking Dropbox read access...";
		try
		{
			accessToken = await GetRequestAccessTokenAsync();
			await dropboxService.TestConnectionAsync(accessToken);
			StatusLabel.Text = "Connected successfully. Dropbox accepted the token and the root folder metadata request. No file contents were downloaded or changed.";
		}
		catch (Exception exception)
		{
			ShowFailure(exception, accessToken, "/2/files/list_folder");
		}
		finally
		{
			SetBusy(false);
		}
	}

	// Tải lại trang đầu của thư mục hiện tại, không tự quay về gốc.
	private async void OnLoadFilesClicked(object? sender, EventArgs eventArgs)
	{
		await LoadPageAsync(false);
	}

	// Nối trang tiếp theo bằng cursor đang giữ; không xóa các mục đã tải.
	private async void OnLoadMoreClicked(object? sender, EventArgs eventArgs)
	{
		await LoadPageAsync(true);
	}

	// Tap trên dòng và nút Open folder dùng chung một luồng kiểm tra/điều hướng.
	private async void OnFolderTapped(object? sender, TappedEventArgs eventArgs)
	{
		await OpenFolderAsync(eventArgs.Parameter as DropboxItem);
	}

	// Nút truyền DropboxItem qua CommandParameter, hỗ trợ cả thao tác bàn phím.
	private async void OnOpenFolderClicked(object? sender, EventArgs eventArgs)
	{
		await OpenFolderAsync((sender as Button)?.CommandParameter as DropboxItem);
	}

	// Tap mở folder hoặc xem .txt; các định dạng khác chưa có trình xem trong sample.
	private async Task OpenFolderAsync(DropboxItem? item)
	{
		if (isBusy || item is null)
		{
			return;
		}

		if (!item.IsFolder)
		{
			await ReadTextAsync(item);
			return;
		}

		if (string.IsNullOrEmpty(item.Path) || !item.Path.StartsWith('/') || item.Path.EndsWith('/'))
		{
			StatusLabel.Text = "This folder has no usable Dropbox path. Reload the list and try again.";
			return;
		}

		await LoadPageAsync(false, item.Path);
	}

	private async void OnReadTextClicked(object? sender, EventArgs eventArgs)
	{
		await ReadTextAsync((sender as Button)?.CommandParameter as DropboxItem);
	}

	// Đọc nội dung không đổi vị trí hoặc cursor; giới hạn phần hiển thị để UI không quá nặng.
	private async Task ReadTextAsync(DropboxItem? file)
	{
		if (isBusy || file is null)
		{
			return;
		}
		ClearTextPreview();
		ClearDiagnostics();
		if (!file.CanReadText)
		{
			StatusLabel.Text = "Only .txt UTF-8 files can be previewed. Other formats are not supported yet.";
			return;
		}
		var accessToken = string.Empty;

		SetBusy(true);
		StatusLabel.Text = "Reading text into memory (maximum 1 MiB)...";
		try
		{
			accessToken = await GetRequestAccessTokenAsync();
			var preview = await dropboxService.ReadTextFileAsync(accessToken, file);
			const int maxDisplayedCharacters = 32_768;
			var displayLength = Math.Min(preview.Content.Length, maxDisplayedCharacters);
			if (displayLength < preview.Content.Length && char.IsHighSurrogate(preview.Content[displayLength - 1]))
			{
				displayLength--;
			}
			PreviewTitleLabel.Text = $"Text preview: {preview.Name}";
			PreviewEditor.Text = preview.Content[..displayLength];
			PreviewStatusLabel.Text = preview.Content.Length == 0
				? $"Empty text file ({preview.Size:N0} bytes). Nothing was saved to disk."
				: $"Read {preview.Size:N0} bytes into memory. " +
					(displayLength < preview.Content.Length ? "Showing only the first 32K characters. " : "Showing all text. ") +
					"Nothing was saved to disk.";
			TextPreviewSection.IsVisible = true;
			StatusLabel.Text = "Text loaded. Scroll to Text preview below the file list. Dropbox files were not changed.";
		}
		catch (Exception exception)
		{
			ShowFailure(exception, accessToken, "/2/files/download");
		}
		finally
		{
			SetBusy(false);
		}
	}

	private void OnClosePreviewClicked(object? sender, EventArgs eventArgs)
	{
		if (!isBusy)
		{
			ClearTextPreview();
		}
	}

	// Xóa nội dung khỏi controls khi đóng, đổi folder hoặc đổi token; không ghi ra file/log.
	private void ClearTextPreview()
	{
		if (TextPreviewSection is not null)
		{
			PreviewEditor.Text = string.Empty;
			PreviewTitleLabel.Text = string.Empty;
			PreviewStatusLabel.Text = string.Empty;
			TextPreviewSection.IsVisible = false;
		}
	}

	// /Docs/Sub -> /Docs; /Docs -> chuỗi rỗng (gốc). Không dùng Path của hệ điều hành.
	private async void OnParentFolderClicked(object? sender, EventArgs eventArgs)
	{
		if (currentFolderPath.Length > 0)
		{
			var separatorIndex = currentFolderPath.LastIndexOf('/');
			await LoadPageAsync(false, currentFolderPath[..separatorIndex]);
		}
	}

	// Quay về gốc bằng lượt liệt kê mới, không tái sử dụng cursor của folder con.
	private async void OnRootFolderClicked(object? sender, EventArgs eventArgs)
	{
		await LoadPageAsync(false, string.Empty);
	}

	// append=true: nối trang hiện tại; false: tải lại hoặc chuyển thư mục bằng trang đầu.
	private async Task LoadPageAsync(bool append, string? folderPath = null)
	{
		if (isBusy || (append && nextCursor is null))
		{
			return;
		}

		ClearDiagnostics();
		var accessToken = string.Empty;

		// Chụp tham số request; chỉ chốt trạng thái điều hướng mới khi nhận kết quả thành công.
		var requestedCursor = append ? nextCursor : null;
		var requestedPath = append ? currentFolderPath : folderPath ?? currentFolderPath;
		var operation = append ? "/2/files/list_folder/continue" : "/2/files/list_folder";
		if (!append)
		{
			ClearTextPreview();
		}

		SetBusy(true);
		EmptyListLabel.Text = "Loading...";
		StatusLabel.Text = append ? "Loading the next page..." : "Loading folder metadata...";
		ListingStatusLabel.Text = StatusLabel.Text;
		try
		{
			accessToken = await GetRequestAccessTokenAsync();
			var page = await dropboxService.ListFilesAsync(accessToken, requestedCursor, requestedPath);
			// Không xóa dữ liệu trước await: lỗi mở folder phải giữ path và danh sách cũ nhất quán.
			if (!append)
			{
				ClearListing();
				currentFolderPath = requestedPath;
				CurrentPathLabel.Text = $"Current folder: {(currentFolderPath.Length == 0 ? "/" : currentFolderPath)}";
			}
			foreach (var item in page.Items)
			{
				if (itemIndexes.TryGetValue(item.Id, out var index))
				{
					items[index] = item;
				}
				else
				{
					itemIndexes.Add(item.Id, items.Count);
					items.Add(item);
				}
			}

			nextCursor = page.NextCursor;
			EmptyListLabel.Text = nextCursor is null
				? "This folder has no files or folders."
				: "This page has no visible items. Select Load more to continue.";
			ListingStatusLabel.Text = nextCursor is null
				? $"Loaded {items.Count} item(s). No more pages."
				: $"Loaded {items.Count} item(s) so far. More pages are available.";
			StatusLabel.Text = "Metadata loaded. No file contents were downloaded or changed.";
		}
		catch (Exception exception)
		{
			ShowFailure(exception, accessToken, operation, requestedCursor);
			// Lỗi continuation có kiểu: bắt đầu lại; lỗi tạm thời giữ cursor để người dùng retry.
			if (append && exception is ApiException<ListFolderContinueError>)
			{
				nextCursor = null;
			}
			EmptyListLabel.Text = "The list could not be completed. See Diagnostic details.";
			ListingStatusLabel.Text = $"Listing failed; the current folder and its {items.Count} item(s) are unchanged. " +
				(append
					? nextCursor is null ? "Select Load files to restart." : "Select Load more to retry this page."
					: "Retry the folder action, or select Load files to reload the current folder.");
		}
		finally
		{
			SetBusy(false);
		}
	}

	// Xóa phiên SecureStorage và dữ liệu trên trang; không thu hồi quyền trên Dropbox.
	private async void OnClearTokenClicked(object? sender, EventArgs eventArgs)
	{
		if (isBusy)
		{
			return;
		}

		SetBusy(true);
		try
		{
			AccessTokenEntry.Text = string.Empty;
			ClearDiagnostics();
			ResetBrowser();
			await authentication.SignOutAsync();
			StatusLabel.Text = "Local session, manual token and list cleared. Dropbox browser login and app authorization were not revoked.";
		}
		catch (Exception exception) { ShowSignInFailure(exception); }
		finally { SetBusy(false); }
	}

	// Đổi token bỏ dữ liệu tài khoản cũ; TextChanged cũng có thể chạy khi XAML đang khởi tạo.
	private void OnAccessTokenChanged(object? sender, TextChangedEventArgs eventArgs)
	{
		if (!isBusy && StatusLabel is not null)
		{
			ClearDiagnostics();
			ResetBrowser();
			StatusLabel.Text = "Not checked. Select Test connection or Load files to verify this token.";
		}
	}

	// Hiện thông báo dễ đọc và chẩn đoán đã che token/cursor, không hiển thị exception thô.
	private void ShowFailure(Exception exception, string accessToken, string operation, string? cursor = null)
	{
		if (authentication.HasSession && exception is AuthException authException &&
			(authException.ErrorResponse?.IsExpiredAccessToken == true || authException.ErrorResponse?.IsInvalidAccessToken == true))
		{
			authentication.MarkAccessTokenExpired();
			StatusLabel.Text = "Dropbox rejected the access token. Retry the action to refresh it, or Disconnect and connect again.";
		}
		else { StatusLabel.Text = GetFailureMessage(exception); }
		if (exception is DropboxAuthenticationException)
		{
			operation = "/oauth2/token";
			if (!authentication.HasSession) { ResetBrowser(); }
		}
		DiagnosticsEditor.Text = DropboxDiagnostics.Create(exception, accessToken, operation, cursor);
		DiagnosticsSection.IsVisible = true;
	}

	// Ánh xạ lỗi SDK/mạng sang gợi ý; xem chi tiết kỹ thuật trong Diagnostic details.
	private static string GetFailureMessage(Exception exception) => exception switch
	{
		DropboxAuthenticationException failure => failure.Message,
		_ when DropboxDiagnostics.GetMissingScope(exception) is string scope =>
			$"The Dropbox app or token lacks {scope}. Enable this scope in the issuing app's Permissions, Submit, then obtain a token or authorization with that scope. Existing tokens do not gain new scopes automatically.",
		AuthException authException when authException.ErrorResponse?.IsExpiredAccessToken == true =>
			"The access token has expired. Generate a new token in Dropbox App Console and try again.",
		AuthException => "Dropbox could not authorize this request. Check the diagnostic details below.",
		AccessException => "Dropbox denied access. Check the app permissions and account restrictions.",
		RateLimitException => "Dropbox is temporarily limiting requests. Wait before trying again.",
		ApiException<ListFolderError> => "Dropbox could not list the requested folder. It may have moved, been deleted, or become inaccessible. Check the diagnostic details below.",
		ApiException<ListFolderContinueError> => "Dropbox could not continue this listing. Select Load files to restart.",
		ApiException<DownloadError> => "Dropbox could not download this file. Reload the folder; the file may be unavailable or require export.",
		InvalidDataException invalidDataException => invalidDataException.Message,
		NotSupportedException notSupportedException => notSupportedException.Message,
		OperationCanceledException => "The request timed out. Check your connection and try again.",
		HttpRequestException => "Could not reach Dropbox. Check your connection and the diagnostic details below.",
		HttpException httpException => $"Dropbox request failed (HTTP {httpException.StatusCode}). Check the diagnostic details below.",
		IOException => "The file could not be read completely. Reload the folder and try again.",
		DropboxException => "The Dropbox SDK reported an error. Check the diagnostic details below.",
		_ => "The operation could not be completed. Check the diagnostic details below."
	};

	// Xóa lỗi cũ trước thao tác mới hoặc khi đổi tài khoản.
	private void ClearDiagnostics()
	{
		if (DiagnosticsEditor is not null && DiagnosticsSection is not null)
		{
			DiagnosticsEditor.Text = string.Empty;
			DiagnosticsSection.IsVisible = false;
		}
	}

	// Reset cả vị trí về gốc khi đổi/xóa token; không dùng để chuyển folder.
	private void ResetBrowser()
	{
		ClearTextPreview();
		currentFolderPath = string.Empty;
		ClearListing();
		if (CurrentPathLabel is not null)
		{
			CurrentPathLabel.Text = "Current folder: /";
			ParentFolderButton.IsEnabled = false;
			RootFolderButton.IsEnabled = false;
		}
	}

	// Chỉ xóa dữ liệu/phân trang, không đổi currentFolderPath; khác với ResetBrowser.
	private void ClearListing()
	{
		items.Clear();
		itemIndexes.Clear();
		nextCursor = null;
		if (LoadMoreButton is not null && ListingStatusLabel is not null && EmptyListLabel is not null)
		{
			LoadMoreButton.IsVisible = false;
			LoadMoreButton.IsEnabled = false;
			ListingStatusLabel.Text = "Select Load files to begin.";
			EmptyListLabel.Text = "No files loaded yet.";
		}
	}

	// Khóa UI trong request; các luồng async gọi lại trong finally để mở UI cả khi lỗi.
	private void SetBusy(bool busy)
	{
		isBusy = busy;
		FilesCollectionView.IsEnabled = !busy;
		ParentFolderButton.IsEnabled = !busy && currentFolderPath.Length > 0;
		RootFolderButton.IsEnabled = !busy && currentFolderPath.Length > 0;
		AccessTokenEntry.IsEnabled = !busy && !authentication.HasSession;
		AppKeyEntry.IsEnabled = !busy && !authentication.HasSession;
		ConnectDropboxButton.IsEnabled = !busy && !authentication.HasSession;
		AuthStatusLabel.Text = authentication.HasSession ? "Signed in with OAuth. Session saved securely; tokens refresh automatically." : "Not signed in with OAuth.";
		TestConnectionButton.IsEnabled = !busy;
		ClearTokenButton.IsEnabled = !busy;
		LoadFilesButton.IsEnabled = !busy;
		LoadMoreButton.IsVisible = nextCursor is not null;
		LoadMoreButton.IsEnabled = !busy && nextCursor is not null;
		ConnectionActivityIndicator.IsVisible = busy;
		ConnectionActivityIndicator.IsRunning = busy;
	}
}
