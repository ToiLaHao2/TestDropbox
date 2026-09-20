using System.Text.Json.Serialization;

namespace MauiApp.Services.Authentication;

// Metadata JSON sinh lúc build để phiên OAuth không phụ thuộc reflection khi linker trim Android.
[JsonSerializable(typeof(DropboxOAuthTokens))]
internal partial class DropboxOAuthJsonContext : JsonSerializerContext
{
}
