using System.Globalization;

namespace MauiApp.Models;

/// <summary>Metadata của một mục trong danh sách; không chứa nội dung file.</summary>
/// <param name="Id">Định danh dùng để cập nhật mục trùng khi nối các trang.</param>
/// <param name="Name">Tên hiển thị trên giao diện.</param>
/// <param name="Path">Đường dẫn Dropbox, không phải đường dẫn ổ đĩa cục bộ; có thể thiếu.</param>
/// <param name="IsFolder">Phân biệt folder để điều hướng thay vì mở file.</param>
/// <param name="Size">Số byte của file; null với folder, 0 với file rỗng.</param>
public sealed record DropboxItem(string Id, string Name, string? Path, bool IsFolder, ulong? Size)
{
	/// <summary>Nhãn loại mục được bind vào giao diện.</summary>
	public string Kind => IsFolder ? "Folder" : "File";

	/// <summary>Chỉ bật xem text cho file .txt; service kiểm tra tiếp dung lượng và UTF-8.</summary>
	public bool CanReadText => !IsFolder && Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);

	/// <summary>Định dạng số byte theo locale; không gán kích thước file cho folder.</summary>
	public string SizeText => Size.HasValue
		? $"{Size.Value.ToString("N0", CultureInfo.CurrentCulture)} bytes"
		: "Size not applicable";
}
