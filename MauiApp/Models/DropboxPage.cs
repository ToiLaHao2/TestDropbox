namespace MauiApp.Models;

/// <summary>Kết quả một lần đọc, không nhất thiết là toàn bộ thư mục.</summary>
/// <param name="Items">File/folder của trang hiện tại; có thể rỗng dù còn trang tiếp.</param>
/// <param name="NextCursor">Null khi hết trang; dùng tiếp cho cùng lượt liệt kê, không ghi vào log thô.</param>
public sealed record DropboxPage(IReadOnlyList<DropboxItem> Items, string? NextCursor);
