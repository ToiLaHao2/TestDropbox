namespace MauiApp.Models;

/// <summary>Nội dung UTF-8 đã đọc vào bộ nhớ; không phải đường dẫn file đã lưu trên thiết bị.</summary>
public sealed record DropboxTextPreview(string Name, string Content, ulong Size);
