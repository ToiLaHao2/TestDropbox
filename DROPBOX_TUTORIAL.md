# Tutorial: Truy cập file Dropbox từ ứng dụng .NET MAUI

Tài liệu làm việc, cập nhật từng bước khi triển khai sample.
**Bản hướng dẫn chính để trả lời diễn đàn:** `MAUI_DROPBOX_TUTORIAL.md`.
File hiện tại giữ nhật ký chi tiết, lịch sử lỗi và kết quả kiểm tra, không thay thế bản cô đọng.
Ngày bắt đầu: 18/09/2026.

**Mốc hiện tại — 18/09/2026:** người dùng đã tạo Dropbox app mới và xác nhận
kiểm tra kết nối thành công. Bước 2.1 đã được triển khai, build và kiểm tra giả lập;
người dùng xác nhận Load files hiển thị các folder trên tài khoản thật.
Bước 2.2 đã được người dùng xác nhận duyệt thư mục thành công.
Bước 2.3 đọc .txt UTF-8 đã triển khai/build và kiểm tra HTTP giả lập; chờ thử UI thật.
Phân trang và các ca UI phụ chưa được xác nhận riêng vẫn để mở.
Vấn đề scope/cấp lại token của app cũ tạm hoãn theo yêu cầu, không coi là đã sửa.

## 1. Mục tiêu và cách cập nhật

Tutorial trả lời câu hỏi: làm thế nào để truy cập các file lưu trên Dropbox
từ một ứng dụng .NET MAUI?

- Phần 1: thiết lập và kiểm tra kết nối với Dropbox.
- Phần 2: duyệt, đọc, tải file và đưa dữ liệu vào ứng dụng; bổ sung upload sau.
- Dùng chung dự án `MauiApp` cho cả hai phần, ưu tiên minh họa trên Windows.
- Chỉ đánh dấu hoàn thành khi bước đó thực sự được thực hiện.
- Mỗi bước ghi thao tác, file hoặc lệnh liên quan, kết quả mong đợi và kết quả kiểm tra.
- Phân biệt rõ kết quả build với kết quả chạy ứng dụng.
- Không ghi access token, refresh token hoặc App secret thật vào tài liệu hay mã nguồn.
- Làm lần lượt: hướng dẫn thao tác → thực hiện → xác nhận kết quả → cập nhật nhật ký → rút gọn hướng dẫn.
- Các thao tác trong Dropbox App Console do người dùng thực hiện; không đánh dấu hoàn thành khi chưa được xác nhận.
- Mục 8 chỉ tổng hợp các bước đã thực hiện và nêu rõ giới hạn kiểm chứng.

## 2. Tiến độ hiện tại

- [x] Tạo dự án .NET MAUI 10 và solution `MauiApp.slnx`.
- [x] Kiểm tra máy có SDK .NET, workload MAUI và các extension VS Code.
- [x] Thêm cấu hình VS Code chạy/debug Windows.
- [x] Sửa xung đột namespace `MauiApp` với kiểu `Microsoft.Maui.Hosting.MauiApp`.
- [x] Build Windows thành công: 0 lỗi, 0 cảnh báo.
- [x] Người dùng đã tạo tài khoản Dropbox và Dropbox app.
- [x] Đã ghi nhận xác nhận ban đầu của người dùng về permission; API sau đó báo thiếu scope, cần kiểm tra lại mục 11.
- [x] Người dùng đã chạy app, nhập token và gửi ảnh thông báo lỗi trên giao diện.
- [x] Người dùng xác nhận Permission type của app ban đầu là Full Dropbox; chưa ghi nhận loại truy cập của app mới.
- [x] Người dùng xác nhận đã tạo access token; không ghi giá trị token vào dự án.
- [x] Khai báo `Dropbox.Api` 7.3.0; xác nhận package có trong bộ nhớ đệm NuGet.
- [x] Triển khai service và màn hình kiểm tra kết nối; build Windows thành công.
- [x] Người dùng đã thử kết nối và cung cấp diagnostics: app ID `8559203` thiếu `files.metadata.read`.
- [x] Thêm diagnostics có che token; kiểm tra dữ liệu giả ở Debug/Release và build Windows riêng thành công.
- [x] Đã nhận diagnostics thật: `BadInputException`, HTTP 400 và scope bị thiếu.
- [x] Bổ sung nhận diện lỗi thiếu scope ở cấp app, không chỉ `AuthException` ở cấp token.
- [x] Người dùng gửi ảnh files.metadata.read có dấu tick xám và log mới AuthException: missing_scope/.
- [ ] Điều tra việc không tạo lại được token ở app cũ — tạm hoãn theo yêu cầu, không chặn phần 2.
- [x] Người dùng đã tạo app mới và xác nhận kiểm tra kết nối thành công ngày 18/09/2026.
- [x] Hoàn thành mục tiêu chính phần 1: kết nối Dropbox; các ca kiểm thử phụ chưa được xác nhận vẫn để mở.
- [x] Triển khai bước 2.1: Load files, Load more, model metadata và các trạng thái.
- [x] Build Windows và kiểm tra phân trang bằng HTTP giả lập thành công.
- [x] Người dùng xác nhận Load files hiển thị folder thực tế ở bước 2.1.
- [ ] Xác nhận hiển thị file, Load more và các ca UI phụ trên tài khoản thật.
- [x] Triển khai bước 2.2: mở folder, Up, Root và tải lại thư mục hiện tại.
- [x] Build bước 2.2: 0 lỗi/0 cảnh báo; 72 kiểm tra service đạt ở mỗi cấu hình Debug/Release.
- [x] Người dùng xác nhận thao tác duyệt thư mục bước 2.2 trên app thật.
- [x] Tạo MAUI_DROPBOX_TUTORIAL.md làm bản hướng dẫn chính, cô đọng cho diễn đàn.
- [x] Triển khai bước 2.3: đọc .txt UTF-8 tối đa 1 MiB, preview văn bản thuần trong bộ nhớ.
- [x] Build Windows 0 lỗi/0 cảnh báo; 94 kiểm tra preview, 72 liệt kê và 23 diagnostics đạt ở mỗi cấu hình Debug/Release.
- [ ] Người dùng xác nhận đọc file text thật, bao gồm tiếng Việt và lỗi thiếu quyền đọc nội dung nếu gặp.
- [ ] Hoàn thành phần 2: thao tác với file.

## 3. Chuẩn bị dự án MAUI — đã thực hiện

### Bước 0.1 — Cấu trúc dự án

- `MauiApp.slnx`: solution.
- `MauiApp/MauiApp.csproj`: cấu hình dự án.
- `MauiApp/MauiProgram.cs`: khởi tạo ứng dụng.
- `MauiApp/MainPage.xaml`: giao diện trang chính.
- `MauiApp/MainPage.xaml.cs`: xử lý tương tác của trang chính.
- `.vscode/launch.json`: cấu hình debug `MAUI - Windows`.

### Bước 0.2 — Sửa lỗi trùng tên MauiApp

Lỗi đã gặp:

```text
CS0118: 'MauiApp' is a namespace but is used like a type
```

Nguyên nhân: tên namespace của dự án trùng với tên lớp MAUI được sử dụng.
Đã thay các tham chiếu kiểu và lời gọi `CreateBuilder()` tương ứng bằng
tên đầy đủ `Microsoft.Maui.Hosting.MauiApp` trong:

- `MauiApp/MauiProgram.cs`.
- `MauiApp/Platforms/Windows/App.xaml.cs`.
- `MauiApp/Platforms/Android/MainApplication.cs`.
- `MauiApp/Platforms/iOS/AppDelegate.cs`.
- `MauiApp/Platforms/MacCatalyst/AppDelegate.cs`.

### Bước 0.3 — Build và chạy Windows

Lệnh build đã kiểm tra, chạy tại thư mục chứa solution:

```powershell
dotnet build MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0 --no-restore --nologo
```

Kết quả: **Build succeeded — 0 Warning(s), 0 Error(s)**.
Lệnh trên sử dụng các package đã restore ở bước thiết lập; không phải lệnh
khởi tạo dành cho máy mới chưa restore package.

Lệnh chạy dành cho bước kiểm tra giao diện tiếp theo, tại thư mục solution:

```powershell
dotnet run --project MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0
```

Hoặc chọn `MAUI - Windows` trong Run and Debug của VS Code rồi nhấn `F5`.
Chưa ghi nhận kết quả kiểm tra giao diện và chưa build các nền tảng khác.

## 4. Phần 1 — Kết nối với Dropbox

**Trạng thái:** người dùng đã tạo Dropbox app mới và xác nhận kết nối thành công.
Hoàn thành mục tiêu chính phần 1, có thể bắt đầu phần 2. Các lỗi scope/token trước
đó được giữ ở mục 10–12 như lịch sử chẩn đoán; việc điều tra app cũ tạm hoãn.
Kết quả này do người dùng xác nhận, không phải một lần gọi API độc lập của trợ lý.

**Mục tiêu hoàn thành:** người đọc cấu hình được Dropbox app, thực hiện
kiểm tra kết nối từ MAUI và nhận thông báo thành công hoặc lỗi rõ ràng.

### Bước 1.1 — Xác nhận cấu hình Dropbox app

- [x] Ghi nhận loại truy cập: **Full Dropbox**, theo xác nhận của người dùng.
- [x] Xác định quyền cần dùng cho sample chỉ đọc.
- [x] Ghi lại thao tác cấu hình và lưu quyền trong App Console; người dùng xác nhận đã thực hiện.
- [ ] Chuẩn bị file `hello.txt` trong phạm vi app được phép truy cập.

Loại truy cập đã xác nhận: **Full Dropbox đối với app ban đầu**.
Tên, App ID và Permission type của app mới chưa được cung cấp; không sao chép
App ID `8559203` hoặc loại truy cập cũ sang app mới theo suy đoán.

**Kết quả:** người dùng xác nhận đã cấp đầy đủ permission ngày 18/09/2026.
Sau đó người dùng xác nhận **Full Dropbox** và đã tạo access token.
Các xác nhận này chưa thay thế việc kiểm tra quyền thực tế bằng API.

**Cập nhật từ API:** request thật bị từ chối vì app ID `8559203` không có
`files.metadata.read`. Chưa biết nguyên nhân thao tác cụ thể là chưa lưu Submit,
cấu hình nhầm app hay đang dùng token của app khác; xem mục 11 để đối chiếu.

**Cập nhật tiếp theo:** ảnh người dùng gửi cho thấy `files.metadata.read` được
chọn (checkbox xám), cùng với `files.metadata.write` đang chọn. Log mới báo
`AuthException: missing_scope/`; cần cấp quyền cho token, không coi checkbox xám
là quyền bị tắt. Xem mục 12; ảnh chưa tự chứng minh thay đổi đã được Submit.

Thao tác trên Dropbox App Console:

1. Mở app Dropbox đã tạo.
2. Trong tab **Settings**, đọc **Permission type** và ghi nhận giá trị
   `App folder` hoặc `Full Dropbox`. Chưa cần đổi loại hoặc tạo lại app.
3. Mở tab **Permissions**, bật `files.metadata.read` và `files.content.read`
   cho demo duyệt và đọc file. Giữ `account_info.read` nếu được bật sẵn/bắt buộc.
4. Lưu thay đổi bằng **Submit** nếu có, rồi kiểm tra lại hai quyền đã chọn.
5. Không bật thêm quyền ghi hoặc xóa cho bản demo chỉ đọc ở bước này.

Kết quả mong đợi: biết loại truy cập của app và xác nhận hai quyền đọc đã lưu.

Kết quả thực tế:

- Permission type: **Full Dropbox**, theo xác nhận của người dùng.
- `files.metadata.read`: **ảnh mới cho thấy đã chọn; log mới báo token thiếu scope. Cần xác nhận đã lưu và cấp token mới có quyền**.
- `files.content.read`: **đã cấp theo xác nhận của người dùng**.
- File `hello.txt`: **chưa chuẩn bị; không bắt buộc cho phép thử kết nối**.
- Token: **người dùng xác nhận đã tạo ở bước 1.2; không lưu giá trị trong tài liệu**.

Chỉ cần phản hồi loại truy cập và trạng thái quyền; không gửi token hoặc App secret.
Nếu cần ảnh minh họa, chỉ chụp vùng liên quan và che thông tin nhạy cảm.

Tài liệu chính thức đã đối chiếu ngày 18/09/2026:

- Dropbox — App Console: `https://docs.dropboxapi.com/dropbox-api/docs/get-started/tutorial/app-console`
- Dropbox — Scopes: `https://dropbox.tech/developers/customizing-scopes-in-oauth-flow`

### Bước 1.2 — Chuẩn bị thông tin kết nối cho demo

- [x] Hướng dẫn tạo token thử nghiệm sau khi cấu hình quyền.
- [x] Người dùng xác nhận đã tạo token thử nghiệm.
- [x] Thêm ô nhập token được che ký tự, chỉ giữ token trong bộ nhớ khi chạy demo.
- [x] Ghi rõ giới hạn của demo dùng token nhập thủ công.

**Mục tiêu:** chuẩn bị access token cho tài khoản thử nghiệm của người phát triển.
Điều kiện: đã lưu các permission cần dùng ở bước 1.1.

Thao tác:

1. Trong Dropbox App Console, mở app đã cấu hình quyền.
2. Chọn tab **Settings**, tìm phần **OAuth 2**.
3. Tại **Generated access token**, nhấn **Generate**. Tạo token sau khi đã lưu
   permission, thay vì dùng lại token tạo trước khi thay đổi quyền.
4. Giữ token riêng trên máy, ngoài thư mục dự án; có thể để trang này mở để
   nhập vào màn hình demo khi bước 1.3 hoàn tất. Không dán vào mã nguồn,
   terminal, tài liệu, ảnh chụp hoặc tin nhắn.
5. Chỉ phản hồi rằng đã tạo token; nếu thấy **Permission type** trong Settings,
   ghi nhận thêm `App folder` hoặc `Full Dropbox` để chuẩn bị dữ liệu sau đó.

Kết quả mong đợi: App Console hiển thị access token dành cho thử nghiệm.
Kết quả ban đầu: token của app cũ bị từ chối vì thiếu scope, như lịch sử mục 11–12.
**Kết quả mới nhất:** người dùng tạo app mới và xác nhận kết nối thành công.
Không lưu token hoặc suy đoán cách người dùng đã tạo token cho app mới.

Lưu ý:

- Token được tạo thủ công phục vụ kiểm thử bằng tài khoản của người phát triển,
  không thay thế luồng đăng nhập OAuth cho người dùng khác.
- Access token có thời hạn. Nếu hết hạn trước lúc thử app, tạo token mới;
  không coi việc tạo token là bằng chứng đã kết nối thành công.
- Không cần nhúng App secret hoặc cấu hình redirect URI cho demo nhập token
  thủ công. OAuth sẽ được hướng dẫn riêng khi triển khai đăng nhập.

Tài liệu tham khảo: Dropbox OAuth Guide, mục Testing with a generated token:
`https://developers.dropbox.com/oauth-guide`.

Định hướng: tách hướng dẫn đăng nhập OAuth thành mục nâng cao;
không coi token thử nghiệm là giải pháp đăng nhập cho khách hàng.

### Bước 1.3 — Tích hợp vào MAUI

- [x] Chọn và ghi lại phiên bản thư viện: `Dropbox.Api` **7.3.0**.
- [x] Thêm thư viện vào dự án; package và các phụ thuộc hiện có trong cache.
- [x] Tạo `DropboxService` để tách xử lý Dropbox khỏi giao diện.
- [x] Thêm nút `Test connection`, `Clear token` và trạng thái đang xử lý.
- [x] Dùng `Files.ListFolderAsync` để kiểm tra quyền đọc metadata.
- [x] Viết các nhánh hiển thị lỗi cố định, không đưa token hoặc exception thô lên màn hình.

#### 1.3.1 — Thêm package

Trong nhóm `PackageReference` của `MauiApp/MauiApp.csproj`, thêm:

```xml
<PackageReference Include="Dropbox.Api" Version="7.3.0" />
```

File trong sample đã có dòng này, không cần thêm lần nữa. Khi thực hiện trên máy
mới, chạy từ thư mục chứa `MauiApp.slnx` để restore riêng target Windows:

```powershell
dotnet restore MauiApp/MauiApp.csproj -p:TargetFrameworks=net10.0-windows10.0.19041.0 --nologo
```

Kết quả mong đợi: restore thành công và có `Dropbox.Api/7.3.0` trong dependency graph.
Kết quả tại máy làm việc: lệnh tải qua công cụ bị chặn mạng, sau đó yêu cầu cấp
quyền cũng lỗi. Khi kiểm tra lại, package và các phụ thuộc đã có sẵn trong cache,
và `MauiApp/obj/project.assets.json` đã chứa `Dropbox.Api/7.3.0`.
Không khẳng định lần restore bị chặn đã thành công; đã dùng build `--no-restore`
để kiểm chứng mã nguồn bằng package có sẵn, không yêu cầu tải tiếp.

#### 1.3.2 — Tạo service kiểm tra kết nối

Tạo `MauiApp/Services/DropboxService.cs`. Đây là phiên bản hiện tại, đã mở rộng
ở bước 2.1 để dùng chung lời gọi metadata cho kiểm tra kết nối và liệt kê.
TestConnectionAsync vẫn chỉ yêu cầu một mục; hai model mới được giải thích ở bước 2.1:

```csharp
using System.Diagnostics;
using System.Net.Http;
using Dropbox.Api;
using Dropbox.Api.Files;
using MauiApp.Models;

namespace MauiApp.Services;

public sealed class DropboxService
{
    private readonly Func<HttpClient> createHttpClient;

    public DropboxService() : this(() => new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
    {
    }

    internal DropboxService(Func<HttpClient> createHttpClient)
    {
        this.createHttpClient = createHttpClient;
    }

    public async Task TestConnectionAsync(string accessToken)
    {
        await ReadFolderAsync(accessToken, null, 1).ConfigureAwait(false);
    }

    public async Task<DropboxPage> ListFilesAsync(string accessToken, string? cursor = null)
    {
        var result = await ReadFolderAsync(accessToken, cursor, 200).ConfigureAwait(false);
        if (result.HasMore && string.IsNullOrWhiteSpace(result.Cursor))
        {
            throw new InvalidDataException("Dropbox returned more pages without a continuation cursor.");
        }

        var items = new List<DropboxItem>();
        foreach (var entry in result.Entries)
        {
            switch (entry)
            {
                case FileMetadata file:
                    items.Add(new DropboxItem(file.Id, file.Name, file.PathDisplay ?? file.PathLower, false, file.Size));
                    break;
                case FolderMetadata folder:
                    items.Add(new DropboxItem(folder.Id, folder.Name, folder.PathDisplay ?? folder.PathLower, true, null));
                    break;
            }
        }

        return new DropboxPage(items, result.HasMore ? result.Cursor : null);
    }

    private async Task<ListFolderResult> ReadFolderAsync(string accessToken, string? cursor, uint limit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        if (cursor is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cursor);
        }

        var operation = cursor is null ? "/2/files/list_folder" : "/2/files/list_folder/continue";
        using var httpClient = createHttpClient();
        var configuration = new DropboxClientConfig("MauiDropboxTutorial")
        {
            HttpClient = httpClient,
            MaxRetriesOnError = 0
        };

        try
        {
            using var client = new DropboxClient(accessToken.Trim(), configuration);
            return cursor is null
                ? await client.Files.ListFolderAsync(string.Empty, limit: limit).ConfigureAwait(false)
                : await client.Files.ListFolderContinueAsync(cursor).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(DropboxDiagnostics.Create(exception, accessToken.Trim(), operation, cursor));
            throw;
        }
    }
}
```

Giải thích lựa chọn:

- Không lưu token trong field, cấu hình, `Preferences`, `SecureStorage` hoặc file.
- Dùng đường dẫn rỗng để yêu cầu metadata của thư mục gốc trong ngữ cảnh tài khoản
  mà token truy cập; không dùng đường dẫn file cục bộ trên Windows.
- `limit: 1` yêu cầu một trang nhỏ để kiểm tra; đây không phải phép đếm toàn bộ file,
  và API có thể trả số mục hơi lớn hơn giới hạn yêu cầu.
- Chưa xử lý phân trang vì bước này chỉ kiểm tra một yêu cầu thành công,
  không có chức năng hiển thị danh sách đầy đủ.
- Thư mục rỗng vẫn có thể kiểm tra thành công; chưa cần tạo `hello.txt`.
- Lời gọi này kiểm tra `files.metadata.read`, không kiểm chứng `files.content.read`.
- Không gọi thông tin tài khoản nên không phụ thuộc vào `account_info.read`.
- HTTP timeout là 30 giây và tắt retry lỗi tự động trong SDK cho phép thử đơn giản.
- `DropboxClient` và `HttpClient` được dispose sau mỗi lần kiểm tra, kể cả khi lỗi.
  Đây là thiết kế cho thao tác kiểm tra đơn lẻ; phần thao tác file liên tục có thể
  tổ chức lại vòng đời client khi triển khai.
- `DropboxDiagnostics` nằm trong `MauiApp/Services/DropboxDiagnostics.cs`, cùng
  namespace với service. Cần giữ file helper này khi sao chép service vào sample khác.
- `catch` ghi chi tiết đã che token bằng `Debug.WriteLine`, rồi dùng `throw;`
  để giữ nguyên exception/stack trace cho giao diện xử lý; không dùng `throw exception`.

Chữ ký API đã đối chiếu với XML documentation đi kèm package 7.3.0 tại
`%USERPROFILE%/.nuget/packages/dropbox.api/7.3.0/lib/netstandard2.0/Dropbox.Api.xml`.
Repository SDK chính thức: `https://github.com/dropbox/dropbox-sdk-dotnet`.

#### 1.3.3 — Tạo giao diện

Trong `MauiApp/MainPage.xaml`, thay nội dung mẫu Hello World/counter bằng:

| Control | Tên trong code | Vai trò |
| --- | --- | --- |
| Entry | `AccessTokenEntry` | Nhập token, `IsPassword=True`, tắt gợi ý và kiểm tra chính tả |
| Button | `TestConnectionButton` | Gọi `OnTestConnectionClicked` |
| Button | `ClearTokenButton` | Gọi `OnClearTokenClicked` để xóa ô nhập |
| ActivityIndicator | `ConnectionActivityIndicator` | Báo đang chờ phản hồi |
| Label | `StatusLabel` | Hiển thị kết quả hoặc hướng dẫn xử lý lỗi |
| Editor | `DiagnosticsEditor` | Chi tiết lỗi đã che token, chỉ đọc và có thể chọn/copy |
| VerticalStackLayout | `DiagnosticsSection` | Hiện khi lỗi; ẩn và xóa khi sửa/xóa token hoặc kiểm tra lại |

Đổi tên trang trong `MauiApp/AppShell.xaml` thành **Dropbox connection**.
Giao diện dùng tiếng Anh để thuận tiện minh họa cho khách; tài liệu làm việc dùng
tiếng Việt. File XAML trong dự án là bản đầy đủ, không cần chép lại bảng này.

#### 1.3.4 — Nối sự kiện và xử lý lỗi

Trong `MauiApp/MainPage.xaml.cs`:

1. Tạo một field `DropboxService` cho trang và cờ `isChecking`.
   Sample khởi tạo service trực tiếp để giữ phần nhập môn gọn, chưa thêm MVVM/DI.
2. Nút **Test connection** đọc và trim token. Nếu trống, báo lỗi ngay, không gửi API.
3. Khóa ô nhập và hai nút, bật loading để tránh gửi trùng yêu cầu.
4. `await dropboxService.TestConnectionAsync(accessToken)`.
5. Chỉ khi lời gọi trả về thành công mới hiển thị **Connected successfully**.
6. Bắt exception, chọn thông báo ngắn qua `GetFailureMessage`, rồi hiện thêm
   diagnostics đã che token. Phân biệt scope, hết hạn, truy cập bị từ chối,
   rate limit, lỗi liệt kê, timeout, lỗi mạng và HTTP lỗi chung.
   `DropboxDiagnostics.IsMissingMetadataScope` nhận diện cả lỗi scope có cấu trúc
   trong `AuthException` và thông báo thiếu scope cụ thể của `BadInputException`.
7. Trong `finally`, tắt loading và mở lại các control, dù thành công hay thất bại.
8. Khi sửa token, xóa kết quả cũ về **Not checked**, tránh hiểu nhầm token mới đã hợp lệ.
9. Nút **Clear token** xóa nội dung ô nhập; không giữ một Dropbox client đang hoạt động.

Các file đã thay đổi cho bước kết nối:

- `MauiApp/MauiApp.csproj`: tham chiếu package.
- `MauiApp/Services/DropboxService.cs`: lời gọi kiểm tra metadata.
- `MauiApp/Services/DropboxDiagnostics.cs`: thu thập và che thông tin nhạy cảm trong lỗi.
- `MauiApp/MainPage.xaml`: giao diện nhập token và trạng thái.
- `MauiApp/MainPage.xaml.cs`: sự kiện và xử lý lỗi.
- `MauiApp/AppShell.xaml`: tên trang.

Giới hạn bảo mật của demo: che ký tự không phải mã hóa. **Clear token** không thu hồi
token trên Dropbox, không xóa clipboard và không cam kết xóa tức thì mọi bản sao
chuỗi trong bộ nhớ do runtime quản lý. Không chia sẻ token hoặc bật log HTTP chứa
header xác thực. Đóng app không phải thao tác revoke token; OAuth sẽ là phần riêng.

#### 1.3.5 — Build kiểm tra

Từ thư mục gốc dự án, dùng các package đã restore:

```powershell
dotnet build MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0 --no-restore --nologo
```

Kết quả thực tế ngày 18/09/2026, sau khi thêm màn hình kết nối:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Tại thời điểm build ban đầu, chưa có xác nhận chạy UI. Sau đó người dùng báo lỗi
khi nhập token; mục 10 ghi lại thay đổi diagnostics và kết quả kiểm tra mới nhất.
Không có bộ test tự động sẵn trong repository; không coi kết quả build là kết quả
kiểm tra kết nối Dropbox thành công.

#### 1.3.6 — Lỗi đã gặp và cách xử lý

| Lỗi | Nguyên nhân quan sát được | Xử lý/kết quả |
| --- | --- | --- |
| `NU1301`, socket bị chặn khi restore | Công cụ không được truy cập NuGet trong sandbox | Không đổi package hoặc bỏ kiểm tra TLS; kiểm tra cache có sẵn và build `--no-restore` |
| Xin quyền tải package vẫn báo lỗi `404` | Cơ chế xét quyền của môi trường công cụ lỗi dù người dùng đồng ý | Không tiếp tục thử tải bằng công cụ khác; bước build sau đó không cần restore |
| `MAUIX2002` cho các handler, `CS0103` cho `CounterBtn` | Lần áp dụng thay đổi ban đầu chỉ cập nhật XAML; file code-behind có BOM khiến bản vá không khớp | Hoàn tất cập nhật code-behind, bỏ handler counter cũ; build lại đạt 0 lỗi |

Lỗi đồng bộ XAML/code-behind là lỗi trong lúc chỉnh sửa sample, không phải lỗi
Dropbox. Người đọc dùng bản code hoàn chỉnh hiện tại không cần tái hiện lỗi này.

### Bước 1.4 — Kiểm tra và chốt phần 1

- [x] Người dùng xác nhận kiểm tra kết nối thành công với app mới ngày 18/09/2026.
- [ ] Token trống hoặc không hợp lệ: hiển thị thông báo dễ hiểu.
- [ ] Thiếu quyền hoặc mất mạng: hiển thị lỗi phù hợp.
- [ ] Ghi lệnh chạy, kết quả thực tế và ảnh minh họa đã che thông tin nhạy cảm.

**Quy trình kiểm tra để tái hiện hoặc hoàn thiện các ca còn lại:**

1. Mở terminal tại `D:\Case\6006338`. Sau khi đã build thành công ở trên, chạy:

```powershell
dotnet run --project MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 --no-build --no-restore
```

2. Xác nhận trang **Dropbox files**, ô token và các nút; tên trang đã đổi ở bước 2.1.
3. Để trống ô token, nhấn **Test connection**. Mong đợi thông báo yêu cầu nhập token.
4. Dán token thật trực tiếp vào ô **Access token**, không dán vào terminal.
5. Nhấn **Test connection**, chờ phản hồi. Nếu lỗi, đọc **Diagnostic details**;
   chỉ chia sẻ loại exception, mã HTTP, request ID và message sau khi xem lại
   dữ liệu nhạy cảm. Xem quy trình chạy lại bản mới ở mục 10.
6. Nếu thành công, nhấn **Clear token** và xác nhận ô nhập được xóa.
7. Chỉ gửi lại kết quả thành công hoặc nội dung thông báo lỗi an toàn.

Nếu chạy bằng F5, chọn cấu hình **MAUI - Windows**. Lệnh `--no-build` chạy bản build
gần nhất; sau khi sửa mã nguồn phải build lại trước khi dùng lệnh này.

| Tình huống | Kết quả mong đợi | Kết quả thực tế |
| --- | --- | --- |
| Lần nhập token do người dùng cung cấp | Biết kết nối thành công hoặc nguyên nhân lỗi | Diagnostics xác định `BadInputException`, HTTP 400: app thiếu `files.metadata.read` |
| Token trống/khoảng trắng | Yêu cầu nhập token, không gửi request | Chưa thử |
| Token của app mới, có quyền đọc metadata | `Connected successfully...` | Người dùng xác nhận thành công ngày 18/09/2026; chưa cung cấp ảnh/log thành công |
| Token không hợp lệ hoặc bị thu hồi | Thông báo không thể xác thực | Chưa thử |
| Token hết hạn | Yêu cầu tạo token mới | Chưa thử |
| Token thiếu `files.metadata.read` | Hướng dẫn cấp quyền và tạo token mới | Đã nhận log missing_scope của app/token cũ; chưa kiểm tra lại sau khi thay app |
| Mất mạng hoặc request quá thời gian | Thông báo lỗi mạng/timeout, nút được mở lại | Chưa thử |
| Nhấn liên tục khi đang kiểm tra | Chỉ một thao tác đang chạy, control bị khóa | Chưa thử |
| Sửa token sau khi thành công | Trạng thái trở về `Not checked` | Chưa thử |
| Nhấn Clear token | Ô nhập trống; không giữ client để gọi tiếp | Chưa thử |

Không thay đổi permission của app chính chỉ để thử lỗi thiếu scope; nếu cần
kiểm thử âm đầy đủ, dùng app/token thử nghiệm riêng. Không cố tạo hàng loạt
request để gây rate limit. Không cần ghi token vào tài liệu kiểm thử.

**Mốc chốt để tiếp tục:** ngày 18/09/2026, người dùng xác nhận kết nối thành công
sau khi tạo Dropbox app mới. Theo yêu cầu, chuyển sang phần 2 và tạm hoãn điều tra
app cũ. Các ca token trống, Clear token, mất mạng và các ca chưa xác nhận trong bảng
vẫn cần bổ sung trước khi hoàn thiện toàn bộ ma trận kiểm thử gửi khách.

Phép thử hiện tại chỉ gọi metadata. Chưa xác nhận đọc/tải nội dung file, upload,
OAuth hay nguyên nhân cụ thể khiến app cũ không tạo lại được token.
Tạo app mới là cách người dùng đã chọn để tiếp tục demo, không phải khuyến nghị
chung rằng mọi lỗi missing_scope đều phải giải quyết bằng tạo lại app.

## 5. Phần 2 — Các thao tác cơ bản với file

**Điều kiện bắt đầu:** đã đáp ứng theo xác nhận kết nối thành công với app mới.
Bước 2.1 đã xác nhận load folder thật; bước 2.2 đã xác nhận duyệt thư mục thành công.
Bước 2.3 đã triển khai đọc text vào bộ nhớ, chờ xác nhận UI; lưu file/import/upload chưa làm.

| Bước | Chức năng | Kết quả mong đợi | Trạng thái |
| --- | --- | --- | --- |
| 2.1 | Liệt kê file và thư mục | Hiển thị tên, loại, kích thước file; xử lý phân trang | Đã xác nhận load folder thật; file/phân trang chưa xác nhận |
| 2.2 | Duyệt thư mục | Mở thư mục con và quay về thư mục cha | Người dùng xác nhận duyệt thành công; ca biên chưa xác nhận riêng |
| 2.3 | Đọc file văn bản | Chọn `hello.txt` và hiển thị nội dung trong app | Đã triển khai/build và kiểm tra giả lập; chờ thử UI thật |
| 2.4 | Tải file xuống | Lưu vào bộ nhớ cục bộ của app và báo kết quả | Chưa làm |
| 2.5 | Import vào ứng dụng | Sử dụng file đã tải, ví dụ hiển thị ảnh hoặc đọc CSV | Mở rộng |
| 2.6 | Upload file | Chọn file trên thiết bị và gửi lên Dropbox | Mở rộng |

### Bước 2.1 — Liệt kê file và thư mục

**Trạng thái:** đã triển khai/build và kiểm tra service bằng HTTP giả lập.
Người dùng đã xác nhận hiển thị folder thật; hiển thị file và phân trang trên UI
chưa được xác nhận. Những thay đổi hỗ trợ duyệt thư mục được ghi ở bước 2.2.

#### 2.1.1 — Thêm model và phương thức service

| File | Vai trò |
| --- | --- |
| `MauiApp/Models/DropboxItem.cs` | Id, Name, Path, IsFolder, Size; Kind và SizeText để hiển thị |
| `MauiApp/Models/DropboxPage.cs` | Items của một trang và NextCursor |
| `MauiApp/Services/DropboxService.cs` | TestConnectionAsync và ListFilesAsync dùng chung ReadFolderAsync |

`Size` là `ulong?`: file rỗng có giá trị 0, thư mục có giá trị null và hiển thị
**Size not applicable**. Path ưu tiên PathDisplay, dự phòng bằng PathLower.
Giao diện hiển thị tên/loại/kích thước; bước 2.2 bổ sung mở thư mục con, chưa mở file.

Phương thức hiện tại: `ListFilesAsync(string accessToken, string? cursor = null, string folderPath = "")`.
Mã minh họa bước 1.3.2 mô tả giai đoạn kết nối; xem source DropboxService.cs và bước 2.2
cho phiên bản duyệt thư mục hiện tại. Luồng thực hiện:

1. Kiểm tra token/cursor trống trước khi gửi HTTP.
2. Không có cursor: gọi `Files.ListFolderAsync(folderPath, limit: 200)`; đường dẫn rỗng là gốc.
3. Có cursor: gọi `Files.ListFolderContinueAsync(cursor)`, giữ nguyên giá trị cursor.
4. Chuyển FileMetadata/FolderMetadata thành DropboxItem.
5. HasMore là true thì trả NextCursor, false thì trả null. Nếu còn trang mà cursor
   trống, báo lỗi thay vì coi là đã tải đủ. Trang rỗng còn cursor vẫn được Load more.
6. Dispose client sau mỗi request; timeout 30 giây, không tự retry lỗi.

200 là giới hạn yêu cầu của một trang, không phải tổng số mục. Test connection
vẫn dùng limit 1 ở gốc. Không quét đệ quy; mở từng thư mục theo thao tác người dùng.
Riêng các request liệt kê không tải nội dung hoặc thay đổi file; đọc text thuộc bước 2.3.
Đây không phải bộ đồng bộ thời gian thực; chọn Load files để đọc lại dữ liệu từ đầu.
Phạm vi App folder/Full Dropbox của app mới chưa ghi nhận; không giả định thư mục
API gốc chứa toàn bộ tài khoản hoặc là thư mục cục bộ Windows.

Constructor internal nhận factory HttpClient để kiểm tra bằng handler giả.
Ứng dụng dùng constructor mặc định; không cần thay config hoặc cài thêm package.

#### 2.1.2 — Hiển thị và quản lý trạng thái

`MainPage.xaml` dùng CollectionView làm vùng cuộn chính, không lồng trong ScrollView:

- Header: token, Test connection, Load files, Clear token và trạng thái.
- ItemTemplate: `x:DataType="models:DropboxItem"`, bind Name, Kind, SizeText.
- Footer: nhãn EmptyListLabel phân biệt chưa tải, đang tải, thư mục rỗng và trang rỗng còn dữ liệu; kèm loading, số mục đã tải, Load more và diagnostics.
- Không dùng CollectionView.EmptyView; nhãn trạng thái rỗng chỉ hiện khi danh sách không có mục.
- SelectionMode=None; bước 2.2 dùng tap trên dòng hoặc nút Open folder để mở thư mục.

Trong `MainPage.xaml.cs`:

1. Bind ObservableCollection vào ItemsSource. Load files thay kết quả/cursor của thư mục hiện tại sau khi request thành công; giữ dữ liệu cũ nếu lỗi.
2. Load more giữ mục cũ và nối trang. Mục trùng Id được cập nhật, không thêm lặp.
3. Khóa các nút/ô nhập trong khi chờ, mở lại trong finally để tránh gửi request trùng.
4. Load more chỉ hiện khi còn cursor; số đếm có chữ **so far** khi chưa hết trang.
5. Lỗi tải thêm vẫn giữ mục đã tải. Lỗi tạm thời giữ cursor để retry; lỗi API
   continuation bỏ cursor và hướng dẫn Load files để bắt đầu lại.
6. Đổi/xóa token sẽ xóa danh sách, cursor và diagnostics cũ để tránh nhầm tài khoản.

Diagnostics ghi đúng endpoint và che thêm cursor ở dạng nguyên văn, URL-encoded,
JSON-escaped và trường cursor phổ biến. Không tự lưu token/cursor/danh sách ra file.
Vẫn xem lại đường dẫn/tên file riêng tư trước khi chia sẻ thông báo lỗi.

**Ghi chú xử lý chuột trên Windows (18/09/2026)**

Người dùng báo không thao tác được bằng chuột trong app. Cấu trúc Header chứa
ô nhập/nút và EmptyView phù hợp với triệu chứng được báo cáo ở dotnet/maui
issue #34432: Header không tương tác được khi EmptyView hiển thị trên Windows.
Đây là nguyên nhân nghi ngờ, chưa tái hiện trực tiếp bằng chuột trong môi trường này.

Đã bỏ CollectionView.EmptyView, chuyển EmptyListLabel xuống Footer và cập nhật
IsVisible theo CollectionChanged (items.Count == 0). Không thay đổi request Dropbox.
Build Windows sau thay đổi thành công: **0 lỗi, 0 cảnh báo**; còn chờ xác nhận UI.

Kiểm tra rút gọn:
1. Đóng app cũ/dừng debugging rồi chạy lại có build (không dùng --no-build).
2. Khi danh sách rỗng, thử click ô token, nhập token và bấm Test connection.
3. Thử Load files, cuộn danh sách rồi Clear token; kiểm tra ô nhập/nút vẫn click được.
4. Trong lúc request chạy, ô nhập/nút tạm bị khóa là chủ ý; sau khi hoàn tất phải mở lại.
5. Sau bước 2.3, click file .txt mở preview; định dạng khác vẫn chưa có trình xem.

#### 2.1.3 — Kiểm tra đã thực hiện

Build Windows ở output riêng để tránh file app có thể đang mở:

```powershell
dotnet build MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0 --no-restore --nologo -o MauiApp/bin/diagnostics-check
```

Kết quả: **0 lỗi, 0 cảnh báo**.

- **45 kiểm tra liệt kê đạt ở Debug và 45 ở Release**, dùng SDK thật với
  HttpMessageHandler giả, không gọi mạng hoặc dùng token của người dùng.
- Bao gồm POST/endpoint/body/auth, limit 1/200, cursor, mapping file/folder/0-byte,
  path fallback, trang rỗng/còn trang, cursor thiếu, lỗi scope/reset/HTTP 500,
  không tự retry, request sau lỗi và che cursor.
- **23 kiểm tra diagnostics cũ cũng đạt ở mỗi cấu hình**.
- Mã kiểm tra tạm ở `MauiApp/obj/listing-smoke` và `MauiApp/obj/diagnostics-smoke`;
  không thêm test framework hoặc test project vào solution.
- Build kiểm chứng XAML/C# biên dịch được; chưa thử tương tác/bố cục UI hoặc dữ liệu
  thật. Chưa build/test Android/iOS/Mac Catalyst trong lượt này.

#### 2.1.4 — Người dùng chạy thử

1. Đóng app cũ/Stop debug để tránh khóa file build.
2. Từ thư mục `D:\Case\6006338`, chạy bản mới, không dùng --no-build:

```powershell
dotnet run --project MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0 --no-restore
```

3. Nhập token của app mới đã kết nối. Có thể Test connection lại; Load files
   không bắt buộc phải nhấn Test connection trước.
4. Nhấn **Load files**, cuộn xuống xem tên, loại và kích thước các mục.
5. Nếu có **Load more**, nhấn để đọc tiếp; nút không xuất hiện khi đã hết trang.
   Không cần tạo hàng trăm file chỉ để thử pagination.
6. Thư mục rỗng là kết quả hợp lệ, không tự coi là lỗi kết nối.
7. Thử Clear token: danh sách cũ phải biến mất. Nếu lỗi, gửi diagnostics đã xem
   lại và che bí mật; không gửi token/tên file riêng tư không cần thiết.

| Ca cần xác nhận | Mong đợi | Trạng thái |
| --- | --- | --- |
| Load files | Hiện danh sách hoặc thông báo rỗng | Người dùng xác nhận hiển thị folder thật ngày 18/09/2026 |
| File/folder | File có số byte, folder không có size file | Chưa thử UI |
| Có nhiều trang | Load more nối thêm, số đếm chưa đủ có chữ so far | Chưa thử UI |
| Hết trang | Ẩn Load more, báo No more pages | Chưa thử UI |
| Lỗi trang tiếp theo | Giữ mục cũ, hướng dẫn retry hoặc tải lại | Chưa thử UI |
| Đổi/xóa token | Xóa cả danh sách và cursor cũ | Chưa thử UI |
| Bấm nhiều nút khi chờ | Không gửi request song song từ trang | Chưa thử UI |

Đã xác nhận load folder ở bước 2.1; chuyển sang **2.2 — mở thư mục con và quay lại**.
Các ca chưa xác nhận trong bảng vẫn để mở, không coi là đã kiểm thử thành công.
Vấn đề app cũ vẫn tạm hoãn. Không đổi quyền Dropbox trong lượt triển khai này.

### Bước 2.2 — Duyệt thư mục

**Trạng thái ngày 18/09/2026:** đã triển khai và build Windows thành công.
Service đã kiểm tra với SDK thật + HTTP giả lập; người dùng đã xác nhận duyệt thư mục thành công.
Các ca nhiều trang, lỗi tải, reset token và folder rỗng vẫn cần xác nhận riêng.
Không thêm package, không lưu token và không đổi quyền Dropbox trong bước này.

#### 2.2.1 — Service nhận đường dẫn

Trong `MauiApp/Services/DropboxService.cs`, giữ tham số cursor ở vị trí thứ hai
để không phá các lời gọi cũ, thêm `string folderPath = ""` ở vị trí thứ ba.

```csharp
var rootPage = await dropboxService.ListFilesAsync(accessToken);
var folderPage = await dropboxService.ListFilesAsync(accessToken, folderPath: "/Docs");
var nextPage = await dropboxService.ListFilesAsync(accessToken, folderPage.NextCursor, "/Docs");
```

Lời gọi cuối chỉ thực hiện khi NextCursor khác null. `/Docs` là ví dụ;
app dùng Path từ metadata folder, không ghép đường dẫn ổ đĩa Windows.
Trang đầu gọi ListFolderAsync(folderPath, limit: 200); trang tiếp gọi
ListFolderContinueAsync(cursor). Khi mở thư mục khác luôn bắt đầu với cursor null.
Test connection vẫn kiểm tra gốc, không làm thay đổi thư mục đang xem.

#### 2.2.2 — Giao diện và trạng thái

Trong `MauiApp/MainPage.xaml`:
1. Thêm nhãn Current folder, nút Up (cha) và Root (gốc); hai nút tắt khi đang ở gốc.
2. Thêm TapGestureRecognizer trên dòng và nút Open folder chỉ hiện với folder.
   Nút cho phép thao tác bằng bàn phím bên cạnh click/tap trên dòng.
3. Giữ nhãn rỗng ở Footer, không đưa CollectionView.EmptyView trở lại.

Trong `MauiApp/MainPage.xaml.cs`:
1. Lưu currentFolderPath; chuỗi rỗng biểu diễn gốc, UI hiển thị `/`.
2. Tap/Open folder kiểm tra IsFolder và Path rồi tải trang đầu. Từ bước 2.3,
   tap file .txt chuyển sang đọc preview; các định dạng khác vẫn chưa hỗ trợ.
3. Up lấy phần đường dẫn trước dấu `/` cuối; Root dùng chuỗi rỗng.
4. Load files tải lại thư mục hiện tại, không tự quay về gốc.
5. Chỉ thay path, danh sách, index chống trùng và cursor sau khi tải thành công.
   Lỗi mở folder giữ thư mục/danh sách/cursor cũ và hiện diagnostics để thử lại.
6. Load more dùng cursor của thư mục đang xem. Lỗi continuation có kiểu
   ListFolderContinueError bỏ cursor và yêu cầu Load files; lỗi tạm thời cho retry.
7. Khi tải, khóa vùng danh sách và controls, chặn request trùng bằng isBusy;
   mở lại trong finally. Đổi/xóa token đưa về gốc, xóa danh sách/cursor/diagnostics.

#### 2.2.3 — Kết quả kiểm tra đã thực hiện

- Build Windows vào output riêng (tránh khóa app đang mở): **0 lỗi, 0 cảnh báo**.
- **72 kiểm tra service đạt ở mỗi cấu hình Debug và Release**, dùng SDK 7.3.0
  với HTTP handler giả, không gọi Dropbox và không dùng token thật.
- Bao gồm kiểm tra cũ và ca đường dẫn con Unicode/khoảng trắng, phân trang bằng
  cursor không gửi path, folder rỗng, folder không tồn tại và gọi lại gốc.
- Mã kiểm tra tạm ở `MauiApp/obj/listing-smoke/ListingSmoke.cs`; đây không phải
  test project được quản lý, có thể mất khi clean. Không coi mock service là xác nhận UI.

#### 2.2.4 — Hướng dẫn chạy và kiểm tra thực tế

1. Đóng app cũ/dừng debugging. Từ thư mục gốc `D:\Case\6006338`, chạy:

```powershell
dotnet run --project MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0 --no-restore
```

2. Nhập token app mới đã dùng thành công, chọn Load files.
3. Click folder hoặc Open folder. Kiểm tra Current folder đổi đúng và thấy
   file/folder bên trong. Dùng một folder có sẵn file để kiểm chứng, không cần gửi token.
4. Mở tiếp folder con; thử Up về cha và Root về gốc.
5. Ở folder con, bấm Load files để xác nhận vẫn ở folder đó; dùng Load more nếu có.
6. Thử folder rỗng: thông báo rỗng hiện, Up/Root vẫn dùng được.
7. Thử mất mạng khi mở folder: đường dẫn và danh sách cũ phải được giữ lại,
   controls mở lại sau lỗi/timeout; có mạng thì click folder lại.
8. Clear token hoặc đổi token: đường dẫn về `/`, danh sách rỗng, nút điều hướng tắt.

| Ca UI | Kết quả xác nhận |
| --- | --- |
| Tap/Open folder | Người dùng xác nhận duyệt thư mục thành công ngày 18/09/2026 |
| Folder nhiều cấp, Up, Root | Chưa thử trên app thật |
| Tải lại và phân trang không trộn thư mục | Chưa thử trên app thật |
| Folder rỗng vẫn thao tác chuột được | Chưa thử trên app thật |
| Lỗi tải giữ trạng thái, controls mở lại | Chưa thử trên app thật |
| Clear token/đổi token reset về gốc | Chưa thử trên app thật |

Chỉ ghi thành công từng ca sau khi người dùng xác nhận. Đã chuyển sang bước 2.3
đọc file văn bản; lưu file xuống thiết bị và import vẫn chưa triển khai.

#### 2.2.5 — Ghi chú trong code và thứ tự đọc

Ngày 18/09/2026: bổ sung comment tiếng Việt theo yêu cầu, không thay đổi hành vi.
XML documentation (`///`) giải thích class, phương thức public và tham số model;
comment ngắn giải thích trách nhiệm từng handler và các quyết định xử lý trạng thái.

| File | Nội dung ghi chú |
| --- | --- |
| `MauiApp/MauiProgram.cs`, `MauiApp/App.xaml.cs` | Khởi tạo app/cửa sổ, font, logging và tránh xung đột tên MauiApp |
| `MauiApp/AppShell.xaml`, `MauiApp/AppShell.xaml.cs` | Shell hiển thị MainPage; Up/Root không chuyển route Shell |
| `MauiApp/Models/DropboxItem.cs`, `MauiApp/Models/DropboxPage.cs` | Metadata khác nội dung file; Size null/0, path, Id và cursor |
| `MauiApp/Services/DropboxService.cs` | Kiểm tra kết nối, trang đầu/trang tiếp, ánh xạ metadata, timeout, client và ném lại lỗi |
| `MauiApp/Services/DropboxDiagnostics.cs` | Lỗi scope, HTTP status khi có, request ID, che token/cursor và giới hạn chẩn đoán |
| `MauiApp/MainPage.xaml.cs` | Các handler, duyệt folder, giữ trạng thái khi lỗi, chống trùng và reset khi đổi token |
| `MauiApp/MainPage.xaml` | Header/template/footer và lý do giữ nhãn rỗng ngoài EmptyView |

Thứ tự đọc để giải thích cho khách:
1. DropboxItem/DropboxPage để hiểu dữ liệu trả về.
2. DropboxService: TestConnectionAsync → ListFilesAsync → ReadFolderAsync.
3. MainPage: handler → LoadPageAsync → cập nhật danh sách/path/cursor hoặc ShowFailure.
4. DropboxDiagnostics để hiểu lỗi nào được hiển thị và phần nào được che.

Điểm cần nhấn mạnh: Clear token chỉ xóa trạng thái trên trang, không thu hồi token
ở Dropbox. Redaction không bảo đảm che mọi dữ liệu riêng tư; phải xem lại tên/path
trước khi chia sẻ. Từ bước 2.3, GetMissingScope phân biệt scope thực tế, không tái dùng
thông báo metadata cho lỗi quyền đọc nội dung. IsMissingMetadataScope chỉ nhận đúng metadata.

Kiểm tra lượt bổ sung comment: đối chiếu 10 file cho thấy không đổi dòng code thực thi;
XAML vẫn parse hợp lệ. Không chạy lại build/test trong lượt chỉ bổ sung comment này;
kết quả build và 72 kiểm tra ở mục 2.2.3 thuộc lượt triển khai trước, không phải chạy mới.

### Bước 2.3 — Đọc file văn bản trong app

**Trạng thái ngày 18/09/2026:** đã triển khai/build và kiểm tra tự động bằng HTTP giả.
Chưa dùng token/file thật để xác nhận trên UI. Không lưu nội dung xuống đĩa và không upload.

#### 2.3.1 — Điều kiện và phạm vi

- Duyệt thư mục đã được người dùng xác nhận thành công.
- Token dùng để đọc nội dung cần `files.content.read`. Nếu API báo thiếu quyền, lưu scope
  trên app cấp token rồi cấp token/ủy quyền có scope mới; không gửi token cho người hỗ trợ.
- Bản đầu chỉ nhận file `.txt` UTF-8 có/không BOM, tối đa **1 MiB = 1.048.576 byte**.
- Không hỗ trợ PDF, Word, UTF-16, file binary, export hoặc xem mọi định dạng ở bước này.

#### 2.3.2 — Code đã bổ sung

1. `DropboxItem.CanReadText` phân biệt file `.txt` (không phân biệt hoa/thường), không áp dụng cho folder.
2. Thêm `Models/DropboxTextPreview.cs`: Name, Content và Size byte của file đã đọc vào bộ nhớ.
3. Thêm `DropboxService.ReadTextFileAsync(accessToken, file, cancellationToken)`:
   - Kiểm tra token, file, Id, định dạng và size đã biết trước khi gọi mạng.
   - Gọi `Files.DownloadAsync(file.Id)`; kiểm tra lại tên/size trong metadata phản hồi.
   - Đọc bằng `GetContentAsStreamAsync`, buffer 8 KiB; chỉ đọc tối đa giới hạn + 1 byte
     để phát hiện vượt cỡ. Không tin hoàn toàn metadata và không đọc stream không giới hạn.
   - Đối chiếu tổng byte với size trả về; lỗi nếu nội dung thiếu/không khớp.
   - Bỏ UTF-8 BOM nếu có, decode strict UTF-8; từ chối byte sai và ký tự NUL.
   - Dùng cancellation token và timeout 30 giây, dispose response/stream/client trong mọi nhánh.
   - Lỗi decoder được thay bằng thông báo chung không có inner exception chứa byte file.
4. Trong MainPage, tap file `.txt` hoặc nút **Read text** gọi service; không đổi path/cursor.
5. Vùng **Text preview** nằm dưới danh sách, dùng Editor read-only cao cố định, tối đa
   32.768 đơn vị ký tự UTF-16, tránh cắt giữa cặp surrogate. UI báo khi chỉ hiển thị phần đầu.
   File rỗng vẫn mở preview với thông báo riêng, không coi là lỗi.
6. **Close preview**, đổi/xóa token, tải lại hoặc yêu cầu chuyển folder xóa nội dung preview.
   Lỗi đọc file không xóa danh sách/path/cursor; UI được mở lại trong finally.
7. Diagnostics ghi `/2/files/download (file content)`; GetMissingScope đọc RequiredScope
   từ AuthException hoặc nhận diện BadInput cho metadata/content.read. Không nhầm quyền đọc
   nội dung thành metadata; không ghi nội dung file vào diagnostics.

Snippet gọi từ handler async (selectedFile lấy từ danh sách, token nhập lúc chạy):

```csharp
var preview = await dropboxService.ReadTextFileAsync(accessToken, selectedFile);
```

Nội dung được đọc vào bộ nhớ để xem không có nghĩa đã triển khai chức năng lưu file bước 2.4.
Việc xóa controls không phải bảo đảm xóa an toàn dữ liệu khỏi bộ nhớ của hệ điều hành.

#### 2.3.3 — Kết quả kiểm tra

- Build Windows ở output riêng: **0 lỗi, 0 cảnh báo**.
- **94 kiểm tra đọc text** đạt ở từng cấu hình Debug/Release bằng SDK thật với HTTP giả.
- Gồm UTF-8 tiếng Việt/emoji qua nhiều chunk, BOM, file rỗng, đúng giới hạn, vượt giới hạn
  theo metadata và theo stream thực, dữ liệu thiếu, UTF-8/UTF-16/NUL không hỗ trợ, file đổi tên,
  lỗi quyền có cấu trúc/BadInput, file không tồn tại, hủy giữa lúc đọc và giải phóng stream.
- Chạy lại **72 kiểm tra liệt kê** và **23 kiểm tra diagnostics** ở từng cấu hình đều đạt.
- Không gọi Dropbox thật, không dùng token thật. Không coi kết quả service là kiểm chứng UI.
- Test tạm: `MauiApp/obj/text-preview-smoke/TextPreviewSmoke.cs`, cùng hai thư mục smoke cũ.
  Đây không phải test project được quản lý, có thể mất sau khi clean.

#### 2.3.4 — Hướng dẫn thử trên app thật

1. Tạo `hello.txt` UTF-8 trong Dropbox bằng công cụ bạn đang dùng, ví dụ nội dung
   `Xin chào Dropbox!`. Dùng dữ liệu thử không nhạy cảm.
2. Đóng app cũ, chạy lại có build từ thư mục gốc repo:

```powershell
dotnet run --project MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0 --no-restore
```

3. Nhập token, Load files, mở folder chứa hello.txt rồi bấm **Read text**.
4. Cuộn xuống cuối danh sách để xem **Text preview**; kiểm tra tiếng Việt đúng và thông báo
   số byte/không lưu xuống đĩa. Kiểm tra Close preview và đổi folder xóa nội dung cũ.
5. Thử file text rỗng, file lớn hơn 1 MiB, file không phải UTF-8 nếu thuận tiện.
   Không thay permission app đang dùng chỉ để tạo lỗi scope; nếu gặp lỗi thật, xem diagnostics.
6. Khi thành công, ghi xác nhận riêng trước khi chuyển sang lưu file bước 2.4.

| Ca UI bước 2.3 | Trạng thái |
| --- | --- |
| Đọc hello.txt UTF-8, hiển thị đúng tiếng Việt | Chờ người dùng xác nhận |
| File rỗng, file vượt giới hạn/encoding không hỗ trợ | Chờ người dùng xác nhận |
| Close preview, đổi folder/token xóa nội dung cũ | Chờ người dùng xác nhận |
| Lỗi mạng/quyền vẫn mở lại controls, giữ danh sách | Chờ người dùng xác nhận |

### Lộ trình tiếp theo

1. **Bước 2.2:** đã xác nhận duyệt thành công; ca biên chưa xác nhận vẫn theo dõi ở mục 2.2.4.
2. **Bước 2.3:** đã triển khai; xác nhận đọc file thật theo mục 2.3.4.
3. **Bước 2.4 — tải xuống:** lưu file vào vùng dữ liệu cục bộ của app, thông báo vị trí;
   xử lý tên file an toàn, trùng tên và file tải dở khi lỗi. Bản đầu chưa cần hộp thoại Save As.
4. **Bước 2.5 — import (mở rộng):** dùng file đã tải để hiển thị ảnh hoặc đọc CSV mẫu;
   đây là xử lý dữ liệu trong app, không chỉ lưu file xuống thiết bị.
5. **Bước 2.6 — upload (mở rộng):** chọn file trên thiết bị, chọn folder đích và xác nhận
   trước khi ghi lên Dropbox; thống nhất cách xử lý trùng tên, không tự ghi đè.

Trước khi chuyển từ demo cá nhân sang app dùng cho khách thực tế, dành một bước riêng
cho đăng nhập OAuth và quản lý phiên/token, thay cho yêu cầu khách tự tạo token thủ công.
Các bước lưu file, import, upload và OAuth là kế hoạch; chưa triển khai trong code.

Thuật ngữ sử dụng trong tutorial:

- **Download:** chuyển file từ Dropbox xuống thiết bị.
- **Import:** ứng dụng đọc và sử dụng dữ liệu trong file.
- **Upload:** chuyển file từ thiết bị lên Dropbox.

Phạm vi bản đầu: **kết nối → duyệt file → đọc văn bản → tải xuống**.
Chưa đưa sửa, xóa, chia sẻ hoặc đồng bộ file vào bản đầu.

## 6. Mẫu ghi lại mỗi bước mới

Khi thực hiện một bước, bổ sung chi tiết ngay dưới mục tương ứng:

```text
Mục tiêu:
Điều kiện trước khi thực hiện:
Các thao tác theo thứ tự:
File đã thêm hoặc sửa:
Lệnh hoặc đoạn code minh họa:
Kết quả mong đợi:
Kết quả kiểm tra thực tế:
Lỗi gặp phải và cách khắc phục:
Tài liệu chính thức đã đối chiếu, nếu có:
```

## 7. Nhật ký làm việc

| Ngày | Nội dung | Kết quả |
| --- | --- | --- |
| 18/09/2026 | Xử lý nghi vấn EmptyView chặn chuột tại Header; chuyển nhãn rỗng xuống Footer | Build Windows: 0 lỗi/0 cảnh báo; chờ người dùng kiểm tra chuột khi danh sách rỗng và sau Clear token |
| 18/09/2026 | Tạo dự án MAUI và cấu hình VS Code | Đã tạo |
| 18/09/2026 | Sửa lỗi xung đột tên `MauiApp` | Build Windows thành công; chưa xác nhận giao diện |
| 18/09/2026 | Thống nhất tutorial gồm kết nối và thao tác file | Đã xác định phạm vi bản đầu |
| 18/09/2026 | Tạo tài liệu hướng dẫn từng bước | File `DROPBOX_TUTORIAL.md` |
| 18/09/2026 | Người dùng xác nhận đã cấp đầy đủ permission | Ghi nhận quyền đọc đã cấu hình; Permission type chưa xác nhận |
| 18/09/2026 | Hướng dẫn tạo access token thử nghiệm | Chờ xác nhận đã tạo; chưa kiểm tra kết nối API |
| 18/09/2026 | Người dùng xác nhận token đã tạo và Permission type là Full Dropbox | Cập nhật bước 1.1 và 1.2, không lưu token |
| 18/09/2026 | Xin quyền restore package sau khi được người dùng đồng ý | Cơ chế xét quyền vẫn lỗi; không tải được qua lệnh này |
| 18/09/2026 | Kiểm tra cache NuGet và assets hiện có | Đã có Dropbox.Api 7.3.0 cùng phụ thuộc; có thể build không restore |
| 18/09/2026 | Thêm service, giao diện và xử lý lỗi | Hoàn thành mã nguồn phần kiểm tra kết nối |
| 18/09/2026 | Build Windows sau khi tích hợp Dropbox | Thành công: 0 lỗi, 0 cảnh báo; chưa thử token thật |
| 18/09/2026 | Bổ sung hướng dẫn chi tiết, rút gọn và ma trận kiểm tra | Chờ người dùng thực hiện bước 1.4 |
| 18/09/2026 | Kiểm chứng ba nguồn tài liệu do người dùng cung cấp | Xác nhận SDK .NET, OAuth Guide và API Explorer là nguồn chính thức; bổ sung mục 9, chưa thử API bằng token |
| 18/09/2026 | Người dùng gửi ảnh lỗi sau khi nhập token | Ghi nhận app đã chạy nhưng thông báo chưa đủ để xác định nguyên nhân |
| 18/09/2026 | Thêm try/catch ở service và vùng Diagnostic details trên giao diện | Hiện exception/message/mã HTTP/request ID khi có; che token trước khi hiển thị hoặc ghi Debug |
| 18/09/2026 | Kiểm tra diagnostics bằng exception giả | 16 kiểm tra đạt ở mỗi cấu hình Debug và Release, không dùng token thật/không gọi mạng |
| 18/09/2026 | Build mặc định bị khóa MauiApp.exe bởi app đang chạy | Không dừng app của người dùng; build thư mục riêng thành công, 0 lỗi/0 cảnh báo |
| 18/09/2026 | Người dùng cung cấp diagnostics thật | HTTP 400, app ID 8559203 thiếu files.metadata.read; không kết luận sai định dạng request |
| 18/09/2026 | Bổ sung nhận diện BadInputException thiếu scope | Hiển thị hướng dẫn đúng app → Permissions → Submit → tạo token mới; giữ nguyên request |
| 18/09/2026 | Kiểm tra lại phần nhận diện và che token | 23 kiểm tra đạt ở Debug và Release; build Windows ở output riêng đạt 0 lỗi/0 cảnh báo |
| 18/09/2026 | Nhận ảnh permission và AuthException missing_scope/ | Quyền đọc có dấu tick; lỗi hiện tại ở scope của token. Chưa biết lý do không tạo lại được token |
| 18/09/2026 | Người dùng tạo Dropbox app mới và xác nhận thành công | Chốt mục tiêu kết nối phần 1; không lưu token, chưa có tên/App ID/Permission type app mới |
| 18/09/2026 | Người dùng yêu cầu tạm hoãn vấn đề app cũ | Giữ lịch sử lỗi, không đánh dấu app cũ đã được sửa; chuyển sang chuẩn bị bước 2.1 |
| 18/09/2026 | Triển khai bước 2.1: Load files và Load more | Thêm model, phân trang, CollectionView, trạng thái và diagnostics che cursor |
| 18/09/2026 | Kiểm tra SDK thật với HTTP giả | 45 kiểm tra liệt kê và 23 kiểm tra diagnostics đạt ở mỗi cấu hình Debug/Release |
| 18/09/2026 | Build Windows sau khi thêm danh sách | 0 lỗi, 0 cảnh báo; chờ người dùng thử Load files trên tài khoản thật |
| 18/09/2026 | Người dùng xác nhận Load files hiển thị folder thật, click folder chưa mở | Ghi nhận kết quả bước 2.1; lập kế hoạch bước 2.2 duyệt thư mục, chưa triển khai |
| 18/09/2026 | Triển khai bước 2.2: đường dẫn, mở folder, Up/Root, tải lại/phân trang theo thư mục | Giữ dữ liệu cũ khi mở folder lỗi; đổi token reset về gốc |
| 18/09/2026 | Build Windows và kiểm tra service sau bước 2.2 | 0 lỗi/0 cảnh báo; 72 kiểm tra đạt ở mỗi cấu hình Debug/Release với HTTP giả; chưa thử UI thật |
| 18/09/2026 | Bổ sung comment tiếng Việt cho service, diagnostics, model, handler UI và khởi tạo app | 10 file chỉ thay comment, XAML hợp lệ; không chạy lại build/test; bổ sung thứ tự đọc code và lộ trình 2.3–2.6 |
| 18/09/2026 | Người dùng xác nhận duyệt thư mục thành công | Chốt mục tiêu cơ bản bước 2.2; không tự đánh dấu mọi ca biên đã đạt |
| 18/09/2026 | Tạo MAUI_DROPBOX_TUTORIAL.md làm tài liệu chính cho diễn đàn | Hai phần kết nối/thao tác, thành phần sample, nguồn SDK/API/OAuth và lưu ý ngắn |
| 18/09/2026 | Triển khai bước 2.3: xem .txt UTF-8 trong bộ nhớ | Giới hạn 1 MiB, UI tối đa khoảng 32K ký tự, xử lý lỗi quyền nội dung và giữ trạng thái duyệt |
| 18/09/2026 | Build và kiểm tra sau bước 2.3 | 0 lỗi/0 cảnh báo; 94 preview + 72 liệt kê + 23 diagnostics đạt ở mỗi cấu hình Debug/Release; chưa thử UI text thật |

**Bước tiếp theo:** chạy bản mới và xác nhận **2.3 — đọc file .txt** theo mục 2.3.4;
sau đó triển khai **2.4 — lưu file xuống thiết bị**.
Load more và các ca UI phụ chưa được người dùng xác nhận vẫn để mở.
Vấn đề app cũ vẫn tạm hoãn.

## 8. Hướng dẫn cô đọng — các bước đã thực hiện

Mục này được bổ sung sau khi có kết quả thực tế; không thay thế nhật ký kiểm tra.

### Chuẩn bị MAUI trên Windows

1. Dùng solution `MauiApp.slnx` và dự án `MauiApp/MauiApp.csproj` đã tạo.
2. Mở thư mục chứa solution trong VS Code; cấu hình debug là `MAUI - Windows`.
3. Nếu đặt namespace là `MauiApp`, dùng tên kiểu đầy đủ
   `Microsoft.Maui.Hosting.MauiApp` ở các điểm khởi tạo để tránh lỗi `CS0118`.
4. Sau khi restore package, build từ thư mục solution:

```powershell
dotnet build MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0 --no-restore --nologo
```

Kết quả build đã ghi nhận: **0 lỗi, 0 cảnh báo**. Sau đó người dùng đã chạy giao diện
và xác nhận kết nối thành công với app mới.

### Kết nối Dropbox

#### Cấu hình permission — đã được người dùng xác nhận

1. Mở app trong Dropbox App Console, chọn **Permissions**.
2. Bật `files.metadata.read` và `files.content.read` cho bản demo chỉ đọc.
3. Lưu thay đổi bằng **Submit** nếu có và kiểm tra lại quyền.

Kết quả ban đầu và lỗi thiếu scope của app cũ được giữ ở mục 11–12.
Kết quả mới nhất: app mới kết nối thành công theo xác nhận của người dùng;
chưa ghi nhận riêng Permission type và toàn bộ danh sách scope của app mới.

#### Chuẩn bị token — đã được người dùng xác nhận

1. Sau khi lưu permission, vào **Settings → OAuth 2 → Generated access token**.
2. Nhấn **Generate** và giữ token riêng để nhập trên giao diện demo.
3. Không đưa token/App secret vào mã nguồn, Git, log hoặc tutorial.

Kết quả mới nhất: người dùng đã dùng app mới để kiểm tra kết nối thành công.
Không ghi giá trị token; vấn đề cấp lại token của app cũ tạm hoãn.

#### Triển khai kết nối — mã nguồn đã build thành công

1. Thêm `Dropbox.Api` 7.3.0 trong file dự án và restore package khi cần.
2. Tạo `DropboxService.TestConnectionAsync` dùng `DropboxClient` và
   `Files.ListFolderAsync(string.Empty, limit: 1)`; dispose client sau mỗi phép thử.
3. Tạo trang có ô token che ký tự, **Test connection**, **Clear token**, loading và status.
4. Kiểm tra token trống trước khi gọi API; khóa control trong khi chờ;
   xử lý lỗi bằng thông báo ngắn và diagnostics đã che token; mở control lại trong `finally`.
5. Build Windows theo bước 1.3.5. Kết quả đã kiểm chứng: **0 lỗi, 0 cảnh báo**.

Chi tiết code, lệnh restore/build và lỗi đã gặp nằm ở bước 1.3.
#### Kết quả xác nhận — kết nối thành công với app mới

1. Người dùng đã tạo Dropbox app mới sau khi gặp trở ngại với app cũ.
2. Người dùng xác nhận kiểm tra kết nối trong sample thành công ngày 18/09/2026.
3. Chốt mốc kết nối; mã liệt kê bước 2.1 đã sẵn sàng để người dùng kiểm tra.

Đây là kết quả đã được người dùng xác nhận, không khẳng định app cũ đã được sửa.
Các ca kiểm thử phụ và thao tác nội dung file vẫn chưa hoàn tất.

### Liệt kê — đã xác nhận load folder thật

1. Thêm DropboxItem/DropboxPage và ListFilesAsync.
2. Load files gọi list_folder tại thư mục hiện tại (ban đầu là gốc); Load more gọi list_folder/continue bằng cursor.
3. Hiển thị CollectionView, có loading/error/empty state và số mục đã tải.
4. Thay token phải xóa danh sách/cursor cũ; lỗi tải thêm không làm mất mục đã có.
5. Build/kiểm tra giả lập đã đạt; người dùng xác nhận load folder thật. Hiển thị file,
   phân trang và các ca UI phụ vẫn cần kiểm tra theo bước 2.1.4.

### Duyệt thư mục — người dùng đã xác nhận thành công

1. Chạy lại app có build, nhập token, Load files ở gốc.
2. Click folder hoặc Open folder để xem nội dung; Current folder hiển thị vị trí.
3. Up về cha, Root về gốc; Load files tải lại vị trí hiện tại, Load more nối trang.
4. Lỗi mở folder giữ dữ liệu cũ; Clear token đưa về gốc và xóa danh sách.
5. Duyệt cơ bản đã được xác nhận; các ca nhiều cấp/rỗng/lỗi và phân trang chưa xác nhận riêng vẫn để mở.

### Đọc text — đã triển khai, chờ xác nhận UI

1. Chuẩn bị hello.txt UTF-8 không quá 1 MiB và token có files.content.read.
2. Chạy lại app, mở folder chứa file, bấm Read text, cuộn xuống Text preview.
3. Đọc văn bản thuần trong bộ nhớ; không lưu xuống đĩa, không ghi nội dung vào log.
4. Close preview/đổi folder/token xóa nội dung; lỗi đọc không làm mất vị trí/danh sách.
5. Build và mock service đã đạt; chờ thử thật. Bản hướng dẫn chính: MAUI_DROPBOX_TUTORIAL.md.

## 9. Nguồn chính thức và cách sử dụng trong tutorial

Ngày đối chiếu: **18/09/2026**. Chỉ kiểm tra tài liệu và mã nguồn công khai,
không đăng nhập Dropbox, không thu thập token và không gửi request xác thực.

### 9.1 — Kết luận về ba nguồn được cung cấp

| Nguồn | Kết luận và căn cứ | Vai trò trong tutorial |
| --- | --- | --- |
| GitHub `dropbox/dropbox-sdk-dotnet` [S1] | SDK .NET chính thức, nằm trong tổ chức Dropbox; README dẫn tới package `Dropbox.Api`, tài liệu API và các ví dụ | Nguồn chính để chọn SDK, tra phương thức C# và tham khảo OAuth |
| Dropbox OAuth Guide [S2] | Hướng dẫn của Dropbox Platform Team trên tên miền dành cho developer; cũng được blog kỹ thuật Dropbox giới thiệu [S5] | Giải thích xác thực, scopes, lựa chọn luồng OAuth, PKCE và refresh token |
| Dropbox API v2 Explorer [S3] | Công cụ chính thức: repository Dropbox [S4] dẫn trực tiếp tới đúng địa chỉ GitHub Pages; có bài công bố của Dropbox [S6] | Thử endpoint độc lập với app, xem tham số và phản hồi API |

Không kết luận một trang chính thức chỉ vì nằm trên GitHub/GitHub Pages: cần
đối chiếu tổ chức sở hữu, README và liên kết từ nguồn của nhà cung cấp như trên.

### 9.2 — SDK .NET: áp dụng vào mã nguồn hiện tại

- Sample đang dùng `Dropbox.Api` 7.3.0; không cần đổi thư viện vì các nguồn này.
- README SDK yêu cầu dùng **7.0.0 trở lên** và cảnh báo các bản cũ không còn
  tương thích với máy chủ từ tháng 01/2026. Bản 7.3.0 trong sample đáp ứng yêu cầu
  tối thiểu đó; đây không phải khẳng định nó là phiên bản mới nhất mọi thời điểm.
- Thư mục Examples có ví dụ OAuth Basic và OAuth PKCE. Tham khảo cách gọi SDK,
  nhưng không coi ví dụ OAuth là một giao diện MAUI có thể chép nguyên trạng.
- `MauiApp/Services/DropboxService.cs` hiện gọi
  `Files.ListFolderAsync(string.Empty, limit: 1)` để kiểm tra metadata.
- Trong phần 2, dùng tài liệu SDK và File Access Guide [S7] để xây dựng duyệt
  thư mục/phân trang, rồi triển khai đọc và tải nội dung file theo từng bước.
- Khi mã nguồn nhánh `main` hoặc tài liệu online thay đổi, đối chiếu lại phiên bản
  package được ghim trong `.csproj` và XML documentation cài kèm package.

### 9.3 — OAuth Guide: phân biệt demo với đăng nhập thực tế

- Bản hiện tại nhận access token nhập thủ công; không thực hiện đăng nhập OAuth.
- Guide giúp giải thích vì sao scopes quyết định thao tác được phép, thay vì
  hiểu **Full Dropbox** là tự động có mọi quyền đọc/ghi.
- Khi triển khai đăng nhập cho khách, tham khảo phần PKCE trong guide và ví dụ
  OAuth PKCE của SDK. Thiết kế callback, lưu trữ token và vòng đời ứng dụng MAUI
  phải được kiểm tra riêng trên từng nền tảng.
- Refresh token là phần mở rộng để duy trì truy cập bằng access token ngắn hạn;
  không coi token tạo thủ công là giải pháp đăng nhập lâu dài.
- Không nhúng App secret vào ứng dụng native. Bước OAuth chưa được triển khai
  hoặc kiểm thử trong sample này; không đánh dấu hoàn thành dựa trên việc đọc guide.

### 9.4 — API Explorer: phép thử đối chiếu tùy chọn

Explorer gửi request tới API thật, không phải dữ liệu giả. Repository mô tả đây
là ứng dụng web chạy phía client và host trên GitHub Pages [S4]; trang Explorer
cho phép nhập tham số, gửi lời gọi và xem phản hồi [S3]. Chỉ dùng endpoint đọc
cho phép thử hiện tại, không thử xóa, di chuyển hoặc ghi đè file quan trọng.

Nếu app báo lỗi và cần đối chiếu, có thể thực hiện:

1. Mở đúng địa chỉ Explorer [S3], chọn nhóm endpoint dành cho user.
2. Chọn endpoint `/files/list_folder`, đọc mô tả và quyền cần dùng.
3. Dùng cùng token thử nghiệm đang nhập trong MAUI để đối chiếu có ý nghĩa.
   Chỉ nhập trong phần xác thực của trang chính thức, không đưa token vào URL,
   ảnh chụp hoặc tài liệu. Không dùng bản sao Explorer của bên thứ ba.
4. Đặt các tham số tương ứng với service hiện tại:

```json
{
  "path": "",
  "recursive": false,
  "limit": 1
}
```

5. Gửi yêu cầu và ghi nhận trạng thái thành công/lỗi. Không cần chia sẻ tên file
   riêng tư trong phản hồi. Không coi số mục ở trang đầu là tổng số file.
6. Sau đó thử cùng token và thao tác **Test connection** trong MAUI.

Cách diễn giải để khoanh vùng lỗi (đây là hướng chẩn đoán, không phải kết luận chắc chắn):

- Nếu cả hai đều báo lỗi xác thực/quyền, kiểm tra token, scope và hạn dùng trước.
- Nếu Explorer thành công nhưng MAUI lỗi, ưu tiên kiểm tra token được dán vào app,
  lời gọi SDK và kết nối mạng/proxy của ứng dụng; môi trường browser và app có thể khác nhau.
- Nếu hai phép thử dùng token/app khác nhau, không suy ra permission của chúng giống nhau.
- Explorer thành công không chứng minh app MAUI chạy đúng, cũng không chứng minh
  quyền đọc nội dung file đã hoạt động. Lượt này vẫn chỉ kiểm tra metadata.

**Trạng thái:** mới hướng dẫn, chưa thực hiện phép thử Explorer bằng token thật.
Đây là công cụ hỗ trợ chẩn đoán tùy chọn, không phải bước bắt buộc thay thế bước 1.4.

### 9.5 — Cách đưa nguồn vào bản hướng dẫn gửi khách

1. **Phần kết nối:** dẫn OAuth Guide để giải thích xác thực; dẫn SDK .NET cho cách
   cài package và gọi API. Nêu rõ demo dùng token thủ công.
2. **Phần nghiệp vụ file:** dẫn tài liệu SDK/API và File Access Guide khi giải thích
   metadata, đường dẫn và phân trang. Explorer dùng để minh họa request/response.
3. Phân biệt nội dung tham khảo từ Dropbox với giao diện, service và các bước
   tích hợp MAUI do sample này xây dựng. Không gọi sample của chúng ta là sample MAUI chính thức của Dropbox.
4. Giữ phiên bản package, ngày kiểm chứng và kết quả chạy thực tế trong tutorial;
   chỉ bổ sung ảnh/kết quả API sau khi thử và đã che dữ liệu nhạy cảm.

### 9.6 — Danh mục nguồn

- [S1] SDK .NET chính thức: `https://github.com/dropbox/dropbox-sdk-dotnet`
- [S2] OAuth Guide: `https://developers.dropbox.com/oauth-guide`
- [S3] API Explorer: `https://dropbox.github.io/dropbox-api-v2-explorer/`
- [S4] Repository chính thức của Explorer: `https://github.com/dropbox/dropbox-api-v2-explorer`
- [S5] Dropbox giới thiệu các developer guide: `https://dropbox.tech/developers/new-and-improved-developer-guides`
- [S6] Dropbox công bố API Explorer: `https://dropbox.tech/developers/announcing-the-dropbox-api-v2-explorer`
- [S7] File Access Guide: `https://developers.dropbox.com/dbx-file-access-guide`
- [S8] Tài liệu API của SDK .NET: `https://dropbox.github.io/dropbox-sdk-dotnet/`
- [S9] Dropbox hướng dẫn refresh token: `https://dropbox.tech/developers/using-oauth-2-0-with-offline-access`
- [S10] Error Handling Guide chính thức: `https://developers.dropbox.com/en-us/error-handling-guide`
- [S11] Dropbox Community, nhân viên Dropbox hướng dẫn bấm Submit và cấp lại token cho cùng lỗi thiếu scope: `https://community.dropbox.com/en/discussion/707622/unable-to-check-files-metadata-read/p1`
- [S12] Dropbox giải thích checkbox metadata.read màu xám: `https://community.dropbox.com/en/discussion/720422/how-can-i-get-files-metadata-read-permission/p1`
- [S13] Dropbox giải thích missing_scope ở token và quyền không được cấp hồi tố: `https://community.dropbox.com/en/discussion/626022/missing-scope-files-metadata-read`
- [S14] Dropbox giải thích không tạo được token cá nhân khi bật team scopes: `https://community.dropbox.com/en/discussion/656325/access-token-generate-with-api-in-php`

## 10. Chẩn đoán lỗi kết nối thực tế

**Lịch sử app cũ:** giữ lại để tham khảo. Người dùng đã kết nối thành công bằng
app mới; hiện không tiếp tục điều tra trường hợp cũ.

### 10.1 — Hiện tượng và điều chưa biết

Ngày 18/09/2026, người dùng chạy app, nhập access token và gửi ảnh thông báo:

```text
Dropbox returned an error. Check the service status and try again later.
```

Đây là chuỗi do nhánh `catch (DropboxException)` của sample cũ tự hiển thị,
không phải nguyên văn phản hồi từ Dropbox. Nó chưa chứng minh Dropbox đang gặp
sự cố, cũng chưa chứng minh token không hợp lệ. Chưa có mã HTTP, request ID hoặc
message gốc nên ở thời điểm đó chưa gán nguyên nhân cụ thể.
Sau khi bổ sung diagnostics, người dùng đã cung cấp lỗi chi tiết; kết luận ở mục 11.

### 10.2 — Thay đổi đã thực hiện

1. `DropboxService.TestConnectionAsync` có `try/catch`: ghi diagnostics đã lọc
   qua `Debug.WriteLine`, sau đó `throw;` để UI vẫn nhận exception ban đầu.
2. Thêm `DropboxDiagnostics.Create`: thu thập thời điểm UTC, tên thao tác, phiên bản
   SDK, loại exception, message, request ID và HTTP status nếu exception cung cấp.
3. Duyệt tối đa 5 tầng exception để không bỏ qua nguyên nhân bên trong.
   Stack trace chỉ thêm ở build Debug; không đọc/gửi request headers hoặc token ra log.
4. Không dùng các thuộc tính status của `AuthException`/`RateLimitException`
   đã bị SDK đánh dấu obsolete. Nếu không lấy được mã thực tế, ghi rõ
   **Not exposed by this exception**, không tự suy diễn mã HTTP.
5. Che token hiện nhập (dạng nguyên văn, URL-encoded và JSON-escaped), chuỗi Bearer,
   các trường access/refresh token, client/app secret và authorization phổ biến.
   Giới hạn nội dung còn 8.000 ký tự sau khi che; không xuất lỗi thô nếu regex timeout.
6. UI có vùng **Diagnostic details** chỉ đọc để chọn/copy; mỗi lần thử mới, sửa
   token hoặc nhấn Clear token sẽ xóa diagnostics cũ để tránh nhầm lẫn.

Đây là chẩn đoán dành cho sample, không phải hệ thống logging production.
Thông báo có thể còn đường dẫn cá nhân hoặc dữ liệu khác ngoài các mẫu đã che;
người dùng phải xem lại trước khi chia sẻ. Không gọi `exception.ToString()` hoặc
ghi nguyên header Authorization vào terminal. Sample không tự lưu file log.

### 10.3 — Kết quả kiểm chứng thay đổi

- Build vào đường dẫn mặc định gặp `MSB3026`, rồi `MSB3027`/`MSB3021` vì app đang
  chạy giữ `MauiApp.exe`. Không kill process hoặc đóng app thay người dùng.
- Kiểm tra build ở output riêng để tránh file đang dùng:

```powershell
dotnet build MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0 --no-restore --nologo -o MauiApp/bin/diagnostics-check
```

- Kết quả: **0 lỗi, 0 cảnh báo**. Đây là build kiểm tra, không thay thế app đang mở.
- Chạy kiểm tra cục bộ bằng exception giả: **16 kiểm tra đạt ở Debug và 16 ở Release**.
  Kiểm tra giữ thông tin hữu ích, che các dạng token, nguyên nhân bên trong,
  cắt chuỗi sau khi che và chỉ có stack trace ở Debug. Không có request mạng.
- Script kiểm tra tạm nằm trong `MauiApp/obj/diagnostics-smoke`, không thêm framework
  test hoặc test project vào solution; đây không phải bộ integration test Dropbox.
- Sau lượt kiểm tra này, người dùng đã gửi lỗi chi tiết; xem mục 11.

### 10.4 — Người dùng chạy lại và gửi kết quả

1. Đóng cửa sổ app cũ; nếu đang debug, nhấn Stop trong VS Code.
2. Mở terminal ở thư mục chứa solution. Chạy lại với build, **không dùng `--no-build`**:

```powershell
dotnet run --project MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0 --no-restore
```

3. Nhập token chỉ vào ô **Access token**, nhấn **Test connection**.
4. Nếu lỗi, chọn/copy nội dung trong **Diagnostic details**. Khi chạy F5 ở Debug,
   cũng có thể xem đầu ra `Debug.WriteLine` qua debugger; không yêu cầu nó xuất
   hiện trong terminal của `dotnet run`.
5. Gửi loại exception, HTTP status (nếu có), Dropbox request ID và message đã
   xem lại. Che token, tên file, đường dẫn cá nhân hoặc thông tin riêng tư còn sót.
6. Ghi nhận nguyên nhân và cách sửa chỉ sau khi đối chiếu được dữ liệu này.

### 10.5 — Ba nguồn tài liệu giúp gì cho chính lỗi này?

| Nguồn | Khi chẩn đoán lỗi | Đưa vào tutorial |
| --- | --- | --- |
| SDK .NET [S1, S8] | Tra đúng lớp exception/phương thức/thuộc tính theo phiên bản; không gom mọi lỗi thành sự cố server | Phần cài package, gọi API, bắt lỗi cụ thể, thu thập request ID và đọc thông báo |
| OAuth Guide [S2] | Đối chiếu scopes, loại token và thời hạn nếu message cho thấy vấn đề xác thực | Giải thích giới hạn token thủ công; hướng mở rộng OAuth PKCE và refresh token |
| API Explorer [S3] | Thử cùng token, endpoint và tham số ngoài MAUI để đối chiếu lỗi API với lỗi trong app | Ví dụ request/response cho từng nghiệp vụ và bước troubleshooting tái lập được |

Error Handling Guide [S10] bổ sung cách đọc mã HTTP và phản hồi Dropbox. Không
coi một mã status riêng lẻ là kết luận về nguyên nhân; đối chiếu cả message.

Hướng dẫn cô đọng cho lượt xử lý này: **đóng app cũ → build/chạy bản mới → thử
token → lấy diagnostics đã che dữ liệu → đối chiếu SDK/OAuth/Explorer → ghi kết quả**.
Chưa tự đổi permission, tạo lại Dropbox app hoặc chuyển OAuth chỉ vì lỗi chưa rõ.

## 11. Sửa lỗi thực tế: app thiếu files.metadata.read

**Lịch sử app cũ:** các bước dưới đây chưa được xác nhận giải quyết app cũ.
Người dùng đã chọn tạo app mới và kết nối thành công; không cần thực hiện lại mục
này để bắt đầu phần 2.

### 11.1 — Bằng chứng từ request thật

Người dùng cung cấp diagnostics sau khi chạy bản có thông tin lỗi:

| Trường | Giá trị |
| --- | --- |
| UTC do log ghi nhận | `2026-09-18T03:21:34.5849281+00:00` |
| Endpoint | `/2/files/list_folder` |
| SDK | `Dropbox.Api` 7.3.0 |
| Exception | `Dropbox.Api.BadInputException` |
| HTTP status | `400` |
| Dropbox request ID | `0a71f0b2d5dc41d4b3c31a596aacae3e` |
| App ID do Dropbox trả về | `8559203` |
| Scope bị thiếu | `files.metadata.read` |

Phần message quyết định:

```text
Your app (ID: 8559203) is not permitted to access this endpoint because it does not have the required scope 'files.metadata.read'.
```

**Kết luận:** lỗi đang chặn phép thử là scope của app gắn với token. Không có căn cứ
để sửa path, thêm query/header tự chế hoặc đổi cách serialize request chỉ vì status
là 400. Full Dropbox quy định phạm vi dữ liệu; scope quy định thao tác API được phép.

Chưa xác định thao tác nào dẫn tới thiếu scope: có thể quyền chưa được lưu,
đã sửa app khác hoặc token không thuộc app đang xem. Đây là các khả năng để kiểm tra,
không phải kết luận rằng người dùng chưa từng bật checkbox.

### 11.2 — Thao tác sửa trên Dropbox App Console

1. Mở **đúng Dropbox app đã tạo ra token đang dùng**. Phản hồi API ghi app ID
   `8559203`; nếu có nhiều app, đừng mặc định app đang mở là app của token này.
   App key và App secret không phải giá trị App ID số trong diagnostics.
2. Mở **Permissions**, bật `files.metadata.read` cho phép thử hiện tại. Giữ
   `files.content.read` cho phần đọc/tải file tiếp theo; chưa cần quyền ghi/xóa.
3. Bấm **Submit** để lưu thay đổi. Nếu chỉ tick checkbox mà chưa Submit, chưa
   coi là đã lưu. Tải lại trang và kiểm tra `files.metadata.read` vẫn được chọn.
4. Trong **cùng app**, vào **Settings → OAuth 2 → Generated access token → Generate**
   để tạo token mới sau khi lưu quyền. Không tiếp tục dùng token cũ để đối chiếu.
5. Đóng app MAUI cũ, build/chạy bản mới theo lệnh dưới, nhấn **Clear token**, rồi
   dán token mới vào ô nhập và chọn **Test connection**.

```powershell
dotnet run --project MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0 --no-restore
```

6. Nếu vẫn lỗi thiếu cùng scope, kiểm tra lại cùng app/cùng token và trạng thái
   quyền sau khi tải lại trang. Có thể dùng Explorer với chính token mới để đối chiếu;
   không dùng token do app Explorer khác cấp để kết luận về app này.

Tài liệu Dropbox [S11] có cùng dạng lỗi và hướng dẫn lưu **Submit**, sau đó
cấp lại quyền/token. Xem OAuth Guide [S2] để phân biệt scopes với loại truy cập.
Không gửi token/App secret vào chat; chỉ chia sẻ kết quả hoặc diagnostics đã kiểm tra.

### 11.3 — Thay đổi trong sample

- Giữ nguyên `DropboxService.TestConnectionAsync` và lời gọi
  `Files.ListFolderAsync(string.Empty, limit: 1)`. Client không tự cấp được scope
  của app bằng việc thêm một tham số vào request.
- Thêm `DropboxDiagnostics.IsMissingMetadataScope`: nhận diện cả lỗi
  `AuthException.ErrorResponse.IsMissingScope` và `BadInputException` có cụm
  `does not have the required scope 'files.metadata.read'`.
- UI kiểm tra trường hợp này trước nhánh HTTP chung và hướng dẫn đúng thao tác
  App Console → Permissions → Submit → token mới.
- Không hardcode app ID vào code; ID vẫn được xem trong diagnostics khi server trả về.
- Nhánh BadInput dùng thông báo văn bản quan sát được, không gán mọi HTTP 400
  thành thiếu scope. Nếu Dropbox thay cách diễn đạt, vẫn còn diagnostics gốc đã che token.

### 11.4 — Kết quả kiểm tra và trạng thái

- Kiểm tra cục bộ với exception giả tái hiện message người dùng: **23 kiểm tra đạt
  ở Debug và 23 ở Release**. Bao gồm lỗi scope đúng, app ID khác/case khác,
  scope khác, bad input không liên quan và việc giữ nguyên diagnostics/che token.
- Build Windows ở `MauiApp/bin/diagnostics-check`: **0 lỗi, 0 cảnh báo**.
- Chưa sửa quyền thay người dùng, chưa có token mới và chưa xác nhận kết nối thành công.

Hướng dẫn cô đọng: **đúng app cấp token → bật files.metadata.read → Submit → tải
lại kiểm tra → Generate token mới trong cùng app → Clear token cũ → Test connection**.
Chỉ đánh dấu hoàn tất phần kết nối sau khi người dùng xác nhận API thành công.

## 12. Quyền đã tick nhưng token vẫn báo missing_scope

**Trạng thái cập nhật:** tạm hoãn theo yêu cầu của người dùng. App mới đã kết nối
thành công; không yêu cầu thêm ảnh Generate của app cũ ở bước hiện tại.

### 12.1 — Dữ liệu mới do người dùng cung cấp

- Ảnh Permissions: `files.metadata.write` tick xanh; `files.metadata.read` tick xám.
- UTC trong log: `2026-09-18T03:30:47.1985710+00:00`.
- Exception: `Dropbox.Api.AuthException`.
- Message: `missing_scope/`.
- Dropbox request ID: `78e46eb1c5ee474b8c2c1d626aacb067`.
- HTTP status: helper hiển thị `Not exposed by this exception`; không suy diễn
  đây là lỗi không có phản hồi mạng hoặc tự điền một mã HTTP chưa ghi nhận.
- Người dùng cho biết không tạo lại được access token, nhưng chưa có ảnh/thông báo
  tại phần Generated access token để xác định lý do.

### 12.2 — Diễn giải đúng checkbox và hai tầng quyền

Dropbox xác nhận [S12] checkbox có dấu tick nhưng bị làm xám vẫn là scope đang
được chọn. Nó bị khóa khi scope khác đang bật phụ thuộc vào quyền này; không cần
cố làm ô đó chuyển sang xanh. Ảnh riêng lẻ chưa xác nhận đã Submit và đang xem
đúng app cấp token.

Phân biệt hai loại lỗi đã gặp:

| Lượt | Dữ liệu thực tế | Diễn giải |
| --- | --- | --- |
| Trước | BadInputException, app thiếu files.metadata.read | Dropbox chặn quyền ở cấu hình app gắn với token |
| Mới | AuthException, missing_scope/ | Token đang gửi thiếu scope được cấp, dù app có thể đã được phép dùng scope đó |

Theo Dropbox [S13], bật thêm quyền trong App Console không tự bổ sung quyền vào
access token đã tạo trước đó. Vì vậy ảnh đã tick và token vẫn thiếu scope không
mâu thuẫn. Endpoint hiện dùng là list_folder, cần quyền đọc metadata; không đổi
payload, path, tên user-agent hoặc SDK để thay thế việc cấp quyền cho token.

Đây là tiến triển trong chẩn đoán, chưa phải xác nhận kết nối thành công.
Log mới không có App ID, nên không dùng nó để khẳng định đang xem cùng app trên Console.

### 12.3 — Làm rõ việc không tạo lại được token

1. Xác nhận quyền đã được Submit trong đúng app. Không cần tắt/bật checkbox xám
   hoặc thu hồi quyền chỉ để làm thay đổi màu của checkbox.
2. Người dùng gửi ảnh **chỉ vùng Settings → OAuth 2 → Generated access token**,
   gồm nút Generate và thông báo liên quan. Che hoàn toàn token/App secret;
   không chụp toàn bộ trang chứa thông tin bí mật.
3. Chưa có ảnh này thì không kết luận tài khoản chỉ được tạo token một lần,
   không yêu cầu xóa app hoặc revoke các kết nối đang dùng.
4. Nếu thông báo nhắc tới team administrator/team account, kiểm tra app có bật
   team scopes không. Nhân viên Dropbox đã giải thích trường hợp này [S14].
   Đây chỉ là khả năng cần kiểm tra, chưa phải nguyên nhân đã xác nhận của người dùng.
5. Với app chỉ dành cho demo cá nhân, chỉ cần các quyền người dùng phục vụ chức năng
   đang làm; không bật hàng loạt quyền team/admin. Không tự tắt quyền của app đang
   phục vụ ứng dụng khác vì có thể ảnh hưởng các chức năng của ứng dụng đó.
6. Nếu không dùng được Generate, có thể cấp token qua luồng OAuth có yêu cầu các
   scope cần thiết [S2]. Cần xác định trở ngại cụ thể trước khi chọn hướng này;
   chưa triển khai OAuth, chưa xin App secret và chưa dùng token của người dùng.

### 12.4 — Ghi chú cho bản tutorial cô đọng

**Scope đã chọn trên app ≠ scope đã cấp cho token cũ.** Checkbox tick xám không
có nghĩa quyền bị tắt. Sau khi lưu thay đổi, cần một lần cấp quyền/token mới có
scope yêu cầu. Nếu Generate không dùng được, thu thập thông báo tại đó trước
khi hướng dẫn tiếp; không lặp lại yêu cầu bấm một nút mà người dùng không sử dụng được.

Lượt này chỉ cập nhật tài liệu và chẩn đoán, không đổi request hoặc mã nguồn,
không chạy lại build và chưa có kết quả thử với token mới.
