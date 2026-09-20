# .NET MAUI + Dropbox: kết nối và thao tác file

Bản hướng dẫn chính, ngắn gọn để chuẩn bị câu trả lời trên diễn đàn.
Nhật ký triển khai, lỗi từng gặp và các ca kiểm tra nằm trong `DROPBOX_TUTORIAL.md`.
Cập nhật: **20/09/2026**. Không đưa token, App secret hoặc tên file riêng tư vào bài đăng.

## 1. Mục tiêu và thành phần

Ứng dụng đăng nhập Dropbox bằng **Authorization Code + PKCE**, sau đó dùng SDK .NET
`Dropbox.Api` để liệt kê thư mục/đọc nội dung. Không cần nhập access token thủ công,
không cần App secret, Dropbox Desktop, index.html hay JavaScript chuyển URL fragment.

Sample hiện dùng **.NET MAUI 10**, **Dropbox.Api 7.3.0**, XAML + C# và chạy thử
cho Windows và Android bằng VS Code. Đây là phiên bản của sample, không phải khẳng định bản mới nhất.

Chuẩn bị:
- .NET SDK/workload MAUI phù hợp, VS Code và extension .NET MAUI; máy đáp ứng yêu cầu Windows của MAUI.
- Tài khoản Dropbox, một app trong Dropbox App Console và file mẫu không nhạy cảm.
- App key (định danh công khai) và các redirect URI/scope đã cấu hình cho chính app đó.

| Thành phần trong sample | Trách nhiệm |
| --- | --- |
| `MauiProgram.cs`, `App.xaml.cs`, `AppShell.xaml` | Khởi tạo ứng dụng và hiển thị MainPage |
| `MainPage.xaml` / `.xaml.cs` | Connect/Cancel/Disconnect, thao tác file và thông báo lỗi |
| `Services/Authentication/` | PKCE/state, callback từng nền tảng, đổi code, refresh và SecureStorage |
| `Platforms/Android/DropboxCallbackActivity.cs` | Nhận callback về app, không chạy HTTP listener trong emulator |
| `Services/DropboxService.cs` | Gọi SDK và quản lý tài nguyên HTTP; không phụ thuộc UI |
| `Services/DropboxDiagnostics.cs` | Thông tin lỗi có che token/cursor |
| `Models/DropboxItem.cs`, `DropboxPage.cs` | Metadata của từng mục, trang kết quả và cursor |
| `Models/DropboxTextPreview.cs` | Tên, nội dung UTF-8 và số byte đã đọc vào bộ nhớ |

## 2. Phần A — Kết nối MAUI với Dropbox

### A1. Tạo/cấu hình Dropbox app

1. Mở [Dropbox App Console](https://www.dropbox.com/developers/apps), tạo app dùng scoped access.
2. Chọn phạm vi dữ liệu: **App folder** chỉ vùng riêng của app; **Full Dropbox** cho phạm vi rộng hơn.
   Đây là phạm vi dữ liệu, **không thay thế scope quyền API**. Chọn quyền tối thiểu cần thiết.
3. Trong **Permissions**, bật quyền tương ứng và lưu bằng **Submit**:

| Chức năng | Scope |
| --- | --- |
| Liệt kê file/folder, duyệt thư mục | `files.metadata.read` |
| Đọc nội dung file hoặc tải xuống | `files.content.read` |
| Upload (chỉ bật khi triển khai) | `files.content.write` |

4. Bản OAuth hiện yêu cầu cả `files.metadata.read` và `files.content.read`; bật và Submit trước khi đăng nhập.
5. Trong **Settings → OAuth 2 → Redirect URIs**, thêm chính xác hai URI sau, không thêm dấu `/` cuối:

```text
http://127.0.0.1:52475/authorize
mauidropboxtutorial://oauth/callback
```

6. Sao chép **App key** để nhập trên giao diện sample. **Không dùng App secret hoặc Generated access token cho ô này.**
7. Nếu thêm quyền sau khi đã đăng nhập, Disconnect rồi Connect lại để cấp lại quyền;
   token/refresh token cũ không tự được nâng scope chỉ vì tick thêm trong console.

Windows nhận code qua loopback; Android nhận code qua activity/deep link. Cả hai kiểm tra
`state` và đổi code bằng PKCE S256. Chỉ code xuất hiện trong callback; token được lấy qua HTTPS.
Các URI được khai báo trong `Services/Authentication/DropboxOAuthOptions.cs`; nếu đổi Android
scheme/host/path, phải sửa cả cấu hình IntentFilter và App Console cho khớp.

### A2. Chuẩn bị dự án và SDK

Nếu bắt đầu từ dự án mới, trong terminal VS Code:

```powershell
dotnet new maui -n MauiApp
dotnet add MauiApp/MauiApp.csproj package Dropbox.Api --version 7.3.0
```

Dự án trong repo đã có package, không cần tạo lại. Nếu dùng namespace `MauiApp`,
ở các vị trí cần kiểu hosting hãy viết `Microsoft.Maui.Hosting.MauiApp` đầy đủ
để tránh lỗi CS0118 do trùng tên namespace.

### A3. Đăng nhập và sử dụng phiên OAuth

Chạy sample từ thư mục gốc repo:

```powershell
dotnet run --project MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0
```

1. Nhập **App key** → **Connect Dropbox** → đăng nhập/cấp quyền trong trình duyệt hệ thống.
2. Windows hiện trang thông báo quay lại app; Android tự gọi callback activity để đưa app lên trước.
3. Khi thành công, app lưu phiên vào **SecureStorage**, rồi tự tải danh sách ở gốc.
4. Khi access token gần hết hạn, thao tác file tiếp theo tự dùng refresh token; không cần Generate token.
5. **Cancel sign-in** hủy lần đăng nhập; app cũng tự ngừng chờ sau 3 phút.
   Đóng tab trình duyệt không tự gửi callback hủy: quay về app để bấm Cancel nếu cần.
6. **Disconnect / Clear token** xóa phiên cục bộ và dữ liệu trang. Nó không đăng xuất trình duyệt
   hoặc thu hồi quyền app trong Dropbox; muốn đổi tài khoản có thể cần đăng xuất trên trình duyệt.

Trong VS Code, chọn Android emulator/device làm target MAUI để chạy Android. OAuth chỉ được
cấu hình cho Windows/Android trong sample; iOS/Mac Catalyst chưa có callback tương ứng.

Luồng code: `MainPage → DropboxAuthService → browser/callback → DropboxOAuthProtocol → SecureStorage`.
Phần OAuth dùng HttpClient gọi `/oauth2/token` để kiểm soát timeout/hủy và thông báo lỗi an toàn;
phần file vẫn dùng Dropbox.Api. App key lưu trong Preferences; token/refresh token không lưu ở đó.

### A4. Token thủ công — chế độ phụ để đối chiếu API

Nếu chỉ muốn kiểm tra API độc lập với OAuth, nhập Generated access token của bạn vào ô
**Optional: manual access token** rồi chọn Test connection hoặc Load files. Không cần App key
cho chế độ này; ô token bị khóa khi đang có phiên OAuth. Token thủ công chỉ giữ trong bộ nhớ,
không được tự refresh. Không dán token vào source, terminal, ảnh chụp hoặc bài đăng.

Ví dụ SDK cốt lõi khi đã có token hợp lệ:

```csharp
using var client = new Dropbox.Api.DropboxClient(accessToken);
await client.Files.ListFolderAsync(string.Empty, limit: 1);
```

Request này chỉ thử metadata, không chứng minh quyền đọc nội dung file.

## 3. Phần B — Các chức năng chính

### B1. Liệt kê và duyệt thư mục

1. **Load files** đọc thư mục hiện tại, ban đầu là gốc.
2. Click folder hoặc **Open folder** để xem file/folder bên trong.
3. **Up** về cha, **Root** về gốc; đường dẫn hiện tại hiển thị trên UI.
4. **Load more** nối trang nếu còn cursor; không chỉ lấy trang đầu rồi coi là toàn bộ dữ liệu.
5. **Load files** trong folder con tải lại chính folder đó, không tự quay về gốc.

Các lời gọi service, thực hiện lần lượt trong handler async:

```csharp
var page = await service.ListFilesAsync(accessToken, folderPath: "/Docs");
if (page.NextCursor is not null)
{
    var nextPage = await service.ListFilesAsync(accessToken, page.NextCursor, "/Docs");
}
```

`/Docs` chỉ là ví dụ; app dùng path từ metadata. Trang đầu dùng `ListFolderAsync`,
trang tiếp dùng `ListFolderContinueAsync`. Đổi folder phải bắt đầu lại, không tái dùng
cursor của folder cũ. Path Dropbox không phải đường dẫn `C:\...` hoặc `D:\...`.

### B2. Đọc file văn bản trong app

**Đã triển khai, chờ xác nhận UI thật.** Quyền cần dùng: `files.content.read`.

1. Chuẩn bị `hello.txt` lưu UTF-8 trong Dropbox, có thể chứa tiếng Việt; dùng nội dung không nhạy cảm.
2. Chạy lại app có build, **Load files** và mở folder chứa file.
3. Click file `.txt` hoặc **Read text**; cuộn xuống vùng **Text preview** dưới danh sách.
4. **Close preview** xóa nội dung khỏi vùng xem; đổi token/folder cũng xóa preview.

Service được gọi từ handler:

```csharp
var preview = await service.ReadTextFileAsync(accessToken, selectedFile);
```

`selectedFile` là DropboxItem của file được chọn. Service dùng `Files.DownloadAsync(file.Id)`
và `GetContentAsStreamAsync()`, không đọc toàn bộ stream không giới hạn.
Giới hạn **1 MiB (1.048.576 byte)** áp dụng cả metadata lẫn số byte thực đọc;
UI hiển thị tối đa khoảng **32K ký tự** và báo khi cắt bớt. Chấp nhận UTF-8 có/không BOM,
báo rõ file rỗng; từ chối UTF-8 sai, dữ liệu có NUL và định dạng khác `.txt`.
Thời gian chờ tối đa được cấu hình 30 giây cho request/đọc stream.

Nội dung chỉ được đọc vào bộ nhớ và hiển thị văn bản thuần, không lưu file ra đĩa,
không ghi nội dung vào diagnostics. Endpoint là `/2/files/download`, không phải metadata.
Nếu thiếu quyền, diagnostics phải nêu `files.content.read`, không nhầm với `files.metadata.read`.

### B3. Download, import và upload — các bước sau

| Chức năng | Cách làm dự kiến | Trạng thái |
| --- | --- | --- |
| Download xuống thiết bị | Đọc stream, lưu vào vùng dữ liệu của app; xử lý trùng tên và file tải dở | Chưa triển khai |
| Import | App sử dụng file đã tải, ví dụ đọc CSV hoặc hiển thị ảnh | Chưa triển khai |
| Upload | Chọn file local, chọn folder đích và xác nhận cách xử lý trùng tên trước khi gửi | Chưa triển khai |

Đọc text ở B2 không có nghĩa đã triển khai lưu file hoặc hỗ trợ mọi định dạng.

## 4. API và nguồn tham khảo

| Nguồn | Dùng để làm gì |
| --- | --- |
| [Dropbox .NET SDK chính thức](https://github.com/dropbox/dropbox-sdk-dotnet) | SDK, ví dụ và mã nguồn để đối chiếu cách gọi API |
| [Tài liệu lớp/phương thức SDK](https://dropbox.github.io/dropbox-sdk-dotnet/gh-pages/obj/api/Dropbox.Api.html) | Kiểu request/response, phương thức và exception |
| [Dropbox OAuth Guide](https://developers.dropbox.com/oauth-guide) | Scope, token và thiết kế đăng nhập OAuth thay cho token thủ công |
| [Dropbox HTTP API Reference](https://www.dropbox.com/developers/documentation/http/documentation) | Hợp đồng endpoint, scope, tham số và lỗi |
| [Dropbox API Explorer](https://dropbox.github.io/dropbox-api-v2-explorer/) | Thử endpoint để phân biệt lỗi API/quyền với lỗi giao diện; không dùng token thật trong bài đăng |
| [Microsoft: .NET MAUI + VS Code](https://learn.microsoft.com/dotnet/maui/get-started/installation?view=net-maui-10.0&tabs=visual-studio-code) | Thiết lập môi trường MAUI |
| [Microsoft: MAUI SecureStorage](https://learn.microsoft.com/dotnet/maui/platform-integration/storage/secure-storage?view=net-maui-10.0) | Lưu phiên OAuth bằng cơ chế bảo vệ dữ liệu của nền tảng |
| [Android: deep links](https://developer.android.com/training/app-links/deep-linking) | Intent filter và callback về ứng dụng |
| [RFC 7636](https://www.rfc-editor.org/rfc/rfc7636) | PKCE, verifier và challenge S256 |

Các endpoint trọng tâm: `/2/files/list_folder`, `/2/files/list_folder/continue`,
`/2/files/download`; `/2/files/upload` cho bước upload sau này.
OAuth dùng `https://www.dropbox.com/oauth2/authorize` và `https://api.dropboxapi.com/oauth2/token`.

## 5. Lưu ý và kết quả xác nhận

- `missing_scope` khác token hết hạn; đọc scope yêu cầu và request ID trong diagnostics.
  Kiểm tra đúng app cấp token, lưu permission và cấp quyền/token lại khi cần.
- Không ghi token, cursor hoặc nội dung file vào log. Diagnostics đã che bí mật phổ biến,
  nhưng vẫn phải xem lại tên/path riêng tư trước khi chia sẻ.
- Folder rỗng là kết quả hợp lệ. Size của folder khác file 0 byte. File có thể nằm trong folder con.
- Khóa thao tác lúc request chạy, mở lại trong finally. Lỗi mở folder giữ danh sách/path cũ.
- Không log code, verifier, state hoặc toàn bộ callback URL. Lỗi OAuth chỉ hiện thông báo an toàn,
  không in nguyên HTTP response/exception chứa bí mật.
- Windows báo port bận: đóng bản sample khác; nếu đổi port, sửa cả constant và redirect trong console.
- Android không dùng `127.0.0.1`/`10.0.2.2` cho callback. Nếu hệ điều hành hủy process trong lúc
  đăng nhập, mở app và Connect lại; sample không lưu verifier/state để phục hồi lần đăng nhập dở.
- Android sample tắt backup bằng allowBackup=false để tránh chuyển phiên mã hóa giữa các bản cài đặt.
- Khi phát hành, dùng scheme riêng của ứng dụng hoặc đánh giá verified app links; không dùng chung scheme demo giữa nhiều app.
- Khi cập nhật sample, đóng app cũ và chạy lại có build; không dùng `--no-build` với binary cũ.

**Đã xác nhận bởi người dùng trước khi thêm OAuth:** kết nối bằng token thủ công, load folder và duyệt thư mục thành công.
Không tự coi mọi ca phân trang/lỗi/đổi token đều đã được xác nhận.
**Đã kiểm tra kỹ thuật ngày 20/09/2026:** build Windows và Android đều 0 lỗi/0 cảnh báo.
Mỗi cấu hình Debug/Release đạt 92 kiểm tra OAuth, 94 đọc text, 72 liệt kê và 23 diagnostics.
OAuth dùng HTTP/storage/browser giả, riêng callback Windows được thử qua socket loopback thật.
Chưa đăng nhập Dropbox thật, chưa kiểm tra UI Android hoặc SecureStorage trên thiết bị thật.
**Bước kế tiếp:** cấu hình App Console, thử Connect trên hai nền tảng, khởi động lại để kiểm tra
phiên lưu, rồi thử duyệt/đọc `.txt`. Chỉ đánh dấu đăng nhập thực tế thành công sau khi xác nhận.
