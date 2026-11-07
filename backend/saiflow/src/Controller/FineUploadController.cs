using System.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SleveenAdmin.Saiflow.Utility;
using saiflow.engine.Service;

namespace SleveenAdmin.Saiflow.Controller;

/// <summary>
/// 任务 2 T2-3：BPMN 文件/文本导入（契约对齐 sfdapi FineUpload）。
/// 路由：api/FineUpload/{Action}（前端 Import / import 均可）
/// </summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class FineUploadController : ControllerBase
{
    private static readonly FormOptions DefaultFormOptions = new();
    private readonly IWorkflowService _workflowService;

    public FineUploadController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    /// <summary>POST api/FineUpload/Import — multipart 上传 BPMN XML。</summary>
    [HttpPost]
    public async Task<IActionResult> Import()
    {
        if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            return Ok(new { success = false, Message = "Unsupported media type!" });

        var boundary = MultipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(Request.ContentType),
            DefaultFormOptions.MultipartBoundaryLengthLimit);
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);

        var section = await reader.ReadNextSectionAsync();
        while (section != null)
        {
            if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition)
                && MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
            {
                try
                {
                    using var streamReader = new StreamReader(section.Body);
                    var xmlContent = await streamReader.ReadToEndAsync();
                    var isOk = CreateNewProcess(xmlContent, out var message);
                    return Ok(new { success = isOk, Message = message });
                }
                catch (Exception ex)
                {
                    return Ok(new { success = false, Message = ex.Message });
                }
            }

            section = await reader.ReadNextSectionAsync();
        }

        return Ok(new { success = false, Message = "Unknown other reasons！" });
    }

    /// <summary>POST api/FineUpload/ImportFromText — 粘贴 XML 导入。</summary>
    [HttpPost]
    public IActionResult ImportFromText([FromBody] ImportFromTextRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.XmlContent))
            return Ok(new { success = false, Message = "XmlContent is required." });

        try
        {
            var isOk = CreateNewProcess(request.XmlContent.Trim(), out var message);
            return Ok(new { success = isOk, Message = message });
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, Message = ex.Message });
        }
    }

    private bool CreateNewProcess(string xmlContent, out string message)
    {
        message = string.Empty;
        if (string.IsNullOrEmpty(xmlContent))
        {
            message = "XML root element is required.";
            return false;
        }

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xmlContent);
        if (xmlDocument.DocumentElement == null)
        {
            message = "XML root element is required.";
            return false;
        }

        _workflowService.ImportProcess(xmlContent);
        message = "Import process succeeded.";
        return true;
    }
}

/// <summary>粘贴导入请求体。</summary>
public class ImportFromTextRequest
{
    public string? XmlContent { get; set; }
}
