using System.Text.Json;
using System.Text.Json.Serialization;

namespace SleveenAdmin.Foundation.AspNet;

/// <summary>
/// 将 JSON <c>null</c> / 空串读成 <see cref="Guid.Empty"/>，兼容 Biz 前端新增时 <c>id: null</c>。
/// </summary>
public sealed class NullableGuidJsonConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return Guid.Empty;

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
                return Guid.Empty;
            return Guid.Parse(s);
        }

        return reader.GetGuid();
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
