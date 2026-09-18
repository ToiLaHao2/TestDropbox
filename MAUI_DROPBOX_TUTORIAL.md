# .NET MAUI + Dropbox: kết nối và thao tác file

Bản hướng dẫn chính, ngắn gọn để chuẩn bị câu trả lời trên diễn đàn.
Nhật ký triển khai, lỗi từng gặp và các ca kiểm tra nằm trong `DROPBOX_TUTORIAL.md`.
Cập nhật: **18/09/2026**. Không đưa token, App secret hoặc tên file riêng tư vào bài đăng.

## 1. Mục tiêu và thành phần

Ứng dụng MAUI gọi Dropbox API qua SDK .NET `Dropbox.Api` để liệt kê thư mục,
đọc nội dung và mở rộng sang tải xuống/upload. Không cần cài Dropbox Desktop.

Sample hiện dùng **.NET MAUI 10**, **Dropbox.Api 7.3.0**, XAML + C# và chạy thử
trên Windows bằng VS Code. Đây là phiên bản của sample, không phải khẳng định bản mới nhất.

Chuẩn bị:
- .NET SDK/workload MAUI phù hợp, VS Code và extension .NET MAUI; máy đáp ứng yêu cầu Windows của MAUI.
- Tài khoản Dropbox, một app trong Dropbox App Console và file mẫu không nhạy cảm.
- Token do chính app đó cấp, với các scope tương ứng chức năng cần thử.

| Thành phần trong sample | Trách nhiệm |
| --- | --- |
| `MauiProgram.cs`, `App.xaml.cs`, `AppShell.xaml` | Khởi tạo ứng dụng và hiển thị MainPage |
| `MainPage.xaml` / `.xaml.cs` | Nhập token, thao tác người dùng, loading, danh sách và thông báo lỗi |
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

4. Sau khi lưu quyền, dùng **Generated access token** trong phần OAuth 2 của app để thử với tài khoản của bạn.
   Nếu quyền được thêm sau khi token đã cấp, cần token/ủy quyền mới có quyền tương ứng;
   việc tick quyền trên console không có nghĩa token cũ tự được nâng quyền.
5. Nhập token trực tiếp trên app. Không dán token vào source, lệnh terminal, tutorial hay ảnh chụp.

**Giới hạn:** generated token dành cho demo cá nhân, có thể hết hạn. Khi phát hành cho
khách, cần luồng OAuth cho từng người dùng; xem OAuth Guide về authorization code + PKCE
cho ứng dụng native. Không nhúng App secret vào ứng dụng MAUI phân phối cho người dùng.

### A2. Chuẩn bị dự án và SDK

Nếu bắt đầu từ dự án mới, trong terminal VS Code:

```powershell
dotnet new maui -n MauiApp
dotnet add MauiApp/MauiApp.csproj package Dropbox.Api --version 7.3.0
```

Dự án trong repo đã có package, không cần tạo lại. Nếu dùng namespace `MauiApp`,
ở các vị trí cần kiểu hosting hãy viết `Microsoft.Maui.Hosting.MauiApp` đầy đủ
để tránh lỗi CS0118 do trùng tên namespace.

### A3. Thử kết nối bằng request metadata

Trong sample, lấy token từ ô nhập rồi gọi `DropboxService.TestConnectionAsync`.
Phần SDK cốt lõi bên trong service là:

```csharp
using var client = new Dropbox.Api.DropboxClient(accessToken);
await client.Files.ListFolderAsync(string.Empty, limit: 1);
```

`accessToken` là giá trị nhập lúc chạy, không phải chuỗi bí mật viết sẵn trong code.
Snippet minh họa API; service thực tế bổ sung timeout, try/catch, giải phóng client
và diagnostics. Request này chỉ kiểm tra đọc metadata, không chứng minh quyền đọc nội dung.

Chạy sample từ thư mục gốc repo:

```powershell
dotnet run --project MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0
```

Nhập token → **Test connection**. Nếu lỗi, xem **Diagnostic details** thay vì chỉ kết luận token sai.

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

Các endpoint trọng tâm: `/2/files/list_folder`, `/2/files/list_folder/continue`,
`/2/files/download`; `/2/files/upload` cho bước upload sau này.

## 5. Lưu ý và kết quả xác nhận

- `missing_scope` khác token hết hạn; đọc scope yêu cầu và request ID trong diagnostics.
  Kiểm tra đúng app cấp token, lưu permission và cấp quyền/token lại khi cần.
- Không ghi token, cursor hoặc nội dung file vào log. Diagnostics đã che bí mật phổ biến,
  nhưng vẫn phải xem lại tên/path riêng tư trước khi chia sẻ.
- Folder rỗng là kết quả hợp lệ. Size của folder khác file 0 byte. File có thể nằm trong folder con.
- Khóa thao tác lúc request chạy, mở lại trong finally. Lỗi mở folder giữ danh sách/path cũ.
- Clear token xóa trạng thái trang, không thu hồi token ở phía Dropbox.
- Khi cập nhật sample, đóng app cũ và chạy lại có build; không dùng `--no-build` với binary cũ.

**Đã xác nhận bởi người dùng:** kết nối, load folder và duyệt thư mục thành công.
Không tự coi mọi ca phân trang/lỗi/đổi token đều đã được xác nhận.
**Đã kiểm tra kỹ thuật:** build Windows 0 lỗi/0 cảnh báo; mỗi cấu hình Debug/Release
đạt 94 kiểm tra đọc text, 72 kiểm tra liệt kê và 23 kiểm tra diagnostics với HTTP giả.
Các kiểm tra này không thay thế việc thử token/file thật trên UI.
**Bước kế tiếp:** xác nhận đọc `.txt` trên UI thật rồi làm download xuống thiết bị.
