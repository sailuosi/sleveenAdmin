using System.Text;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;

namespace SleveenAdmin.Web.Api.Swagger;

/// <summary>
/// Microsoft.OpenApi 2.x 的 OpenAPI 3.0 序列化固定输出 3.0.4，旧版 Swagger UI 无法识别。
/// 输出前将版本号降为 3.0.3 以兼容内置 UI。
/// </summary>
public sealed class SwaggerUiCompatDocumentSerializer : ISwaggerDocumentSerializer
{
    public void SerializeDocument(OpenApiDocument document, IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        var buffer = new StringBuilder();
        var textWriter = new StringWriter(buffer);
        var jsonWriter = new OpenApiJsonWriter(textWriter);
        document.SerializeAs(specVersion, jsonWriter);
        textWriter.Flush();

        var json = buffer.ToString().Replace("\"openapi\": \"3.0.4\"", "\"openapi\": \"3.0.3\"", StringComparison.Ordinal);
        writer.WriteRaw(json);
    }
}
