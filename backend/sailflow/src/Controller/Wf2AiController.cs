using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using sailflow.AI.Configuration;
using sailflow.AI.Entity;
using sailflow.AI.Implementation;
using sailflow.AI.Service;
using sailflow.WebUtility;

namespace SleveenAdmin.Sailflow.Controller;

/// <summary>
/// 任务 2 T2-4b：AI 设计器 API（契约对齐前端 settingapi / axconfigapi / agentapi）。
/// Agent* 与 Model* 同源（ai_model_provider）；AxConfig → ai_act_config。
/// 路由：api/Wf2Ai/{Action}
/// </summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class Wf2AiController : ControllerBase
{
    private readonly IAiModelDataService _modelData;

    public Wf2AiController(IAiModelDataService modelData)
    {
        _modelData = modelData;
    }

    #region Model（ai_model_provider）

    /// <summary>GET api/Wf2Ai/GetModelList</summary>
    [HttpGet]
    public ResponseResult<List<AiModelProviderEntity>> GetModelList()
    {
        try
        {
            return ResponseResult<List<AiModelProviderEntity>>.Success(_modelData.GetModelListAll());
        }
        catch (Exception ex)
        {
            return ResponseResult<List<AiModelProviderEntity>>.Error(ex.Message);
        }
    }

    /// <summary>GET api/Wf2Ai/GetModelListByType?modelType=</summary>
    [HttpGet]
    public ResponseResult<List<AiModelProviderEntity>> GetModelListByType([FromQuery] string? modelType)
    {
        try
        {
            return ResponseResult<List<AiModelProviderEntity>>.Success(
                _modelData.GetModelListByType(modelType ?? string.Empty));
        }
        catch (Exception ex)
        {
            return ResponseResult<List<AiModelProviderEntity>>.Error(ex.Message);
        }
    }

    /// <summary>GET api/Wf2Ai/GetModel/{id}</summary>
    [HttpGet]
    public ResponseResult<AiModelProviderEntity> GetModel(string id)
    {
        try
        {
            var entity = _modelData.GetModelById(id);
            if (entity == null)
                return ResponseResult<AiModelProviderEntity>.Error("model not found");
            return ResponseResult<AiModelProviderEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            return ResponseResult<AiModelProviderEntity>.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Ai/SaveModel — Id 非空则更新，否则新增；返回 Entity。</summary>
    [HttpPost]
    public ResponseResult<AiModelProviderEntity> SaveModel([FromBody] JObject? body)
    {
        try
        {
            var entity = body?.ToObject<AiModelProviderEntity>();
            if (entity == null)
                return ResponseResult<AiModelProviderEntity>.Error("body required");
            if (string.IsNullOrWhiteSpace(entity.ModelProvider))
                return ResponseResult<AiModelProviderEntity>.Error("ModelProvider required");

            var modelId = entity.Id?.Trim();
            if (!string.IsNullOrWhiteSpace(modelId))
            {
                var existing = _modelData.GetModelById(modelId);
                if (existing == null)
                    return ResponseResult<AiModelProviderEntity>.Error("model not found");

                // 前端未改密钥时可能传空或掩码；保留库内密文
                if (IsBlankOrMaskedApiKey(entity.ApiKey))
                    entity.ApiKey = existing.ApiKey;
                if (string.IsNullOrEmpty(entity.ApiUUID))
                    entity.ApiUUID = existing.ApiUUID;

                _modelData.UpdateModel(entity);
                var updated = _modelData.GetModelById(modelId);
                return ResponseResult<AiModelProviderEntity>.Success(updated!, "SaveModel ok");
            }

            if (string.IsNullOrWhiteSpace(entity.ApiKey) || IsBlankOrMaskedApiKey(entity.ApiKey))
                return ResponseResult<AiModelProviderEntity>.Error("ApiKey required for new model");

            var newId = _modelData.CreateModel(entity);
            var created = _modelData.GetModelById(newId) ?? entity;
            created.Id = newId;
            return ResponseResult<AiModelProviderEntity>.Success(created, "SaveModel ok");
        }
        catch (Exception ex)
        {
            return ResponseResult<AiModelProviderEntity>.Error(ex.Message);
        }
    }

    /// <summary>GET api/Wf2Ai/DeleteModel/{id}</summary>
    [HttpGet]
    public ResponseResult DeleteModel(string id)
    {
        try
        {
            _modelData.DeleteModel(id);
            return ResponseResult.Success("DeleteModel ok");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Ai/TestModelConnection</summary>
    [HttpPost]
    public async Task<ResponseResult> TestModelConnection([FromBody] JObject? body)
    {
        try
        {
            var request = body?.ToObject<TestModelConnectionRequest>();
            if (request == null
                || string.IsNullOrWhiteSpace(request.BaseUrl)
                || string.IsNullOrWhiteSpace(request.ApiKey)
                || string.IsNullOrWhiteSpace(request.ModelProvider))
            {
                return ResponseResult.Error("BaseUrl / ApiKey / ModelProvider required");
            }

            var plainKey = ResolvePlainApiKey(request.ApiKey, request.ApiUUID);
            var llm = AiLlmServiceFactory.CreateLargeModelServcieTesting(request.ModelProvider, out var modelName);
            var message = await llm.TestConnectionAsync(request.BaseUrl, plainKey, modelName);
            return ResponseResult.Success(message ?? "ok");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    #endregion

    #region Agent（与 Model 同源别名）

    [HttpGet]
    public ResponseResult<AiModelProviderEntity> GetAgent(string id) => GetModel(id);

    [HttpGet]
    public ResponseResult<List<AiModelProviderEntity>> GetAgentListSimple() => GetModelList();

    [HttpPost]
    public ResponseResult<AiModelProviderEntity> SaveAgent([FromBody] JObject? body) => SaveModel(body);

    [HttpGet]
    public ResponseResult DeleteAgent(string id) => DeleteModel(id);

    /// <summary>流程 Upgrade 语义不适用于模型表；返回明确错误。</summary>
    [HttpPost]
    public ResponseResult UpgradeAgent([FromBody] object? body) =>
        ResponseResult.Error("UpgradeAgent is not applicable to ai_model_provider; use SaveModel.");

    #endregion

    #region AxConfig（ai_act_config）

    /// <summary>GET api/Wf2Ai/GetAxConfig?processId=&amp;version=&amp;activityId=</summary>
    [HttpGet]
    public ResponseResult<AiActivityConfigEntity> GetAxConfig(
        [FromQuery] string? processId,
        [FromQuery] string? version,
        [FromQuery] string? activityId)
    {
        try
        {
            var entity = _modelData.GetAiActivityConfigByProcessVersionActivity(
                processId ?? "", version ?? "1", activityId ?? "");
            return ResponseResult<AiActivityConfigEntity>.Success(entity!);
        }
        catch (Exception ex)
        {
            return ResponseResult<AiActivityConfigEntity>.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Ai/SaveAxConfig</summary>
    [HttpPost]
    public ResponseResult SaveAxConfig([FromBody] JObject? body)
    {
        try
        {
            var entity = body?.ToObject<AiActivityConfigEntity>();
            if (entity == null
                || string.IsNullOrWhiteSpace(entity.ProcessId)
                || string.IsNullOrWhiteSpace(entity.ActivityId))
            {
                return ResponseResult.Error("ProcessId and ActivityId required");
            }

            if (string.IsNullOrEmpty(entity.Version))
                entity.Version = "1";

            _modelData.SaveAiActivityConfigInfo(entity);
            return ResponseResult.Success("SaveAxConfig ok");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>GET/DELETE api/Wf2Ai/DeleteAxConfig</summary>
    [HttpGet]
    [HttpDelete]
    public ResponseResult DeleteAxConfig(
        [FromQuery] string? processId,
        [FromQuery] string? version,
        [FromQuery] string? activityId)
    {
        try
        {
            _modelData.DeleteAiActivityConfig(processId ?? "", version ?? "1", activityId ?? "");
            return ResponseResult.Success("DeleteAxConfig ok");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>
    /// GET api/Wf2Ai/GetTableNames — 依赖 Supabase RPC；未配置时返回空列表（设计器可继续）。
    /// </summary>
    [HttpGet]
    public ResponseResult<List<string>> GetTableNames()
    {
        try
        {
            var list = new KbDocumentService().GetTableNames();
            return ResponseResult<List<string>>.Success(list);
        }
        catch (Exception)
        {
            return ResponseResult<List<string>>.Success(new List<string>(), "Supabase list_tables unavailable");
        }
    }

    /// <summary>GET api/Wf2Ai/GetKnowledgeBaseDocuments — 对齐 ClientApp kbmanager。</summary>
    [HttpGet]
    public ResponseResult<List<KbDocumentEntity>> GetKnowledgeBaseDocuments()
    {
        try
        {
            var list = new KbDocumentService().GetAllDocuments();
            return ResponseResult<List<KbDocumentEntity>>.Success(list);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<KbDocumentEntity>>.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Ai/SearchKnowledgeBase · { Query, Threshold, Count }</summary>
    [HttpPost]
    public ResponseResult<List<KbDocumentEntity>> SearchKnowledgeBase([FromBody] JObject? body)
    {
        try
        {
            var query = body?["Query"]?.ToString() ?? body?["query"]?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(query))
                return ResponseResult<List<KbDocumentEntity>>.Error("Query required");

            var threshold = 0.7f;
            if (body?["Threshold"] != null) float.TryParse(body["Threshold"]!.ToString(), out threshold);
            else if (body?["threshold"] != null) float.TryParse(body["threshold"]!.ToString(), out threshold);

            var count = 5;
            if (body?["Count"] != null) int.TryParse(body["Count"]!.ToString(), out count);
            else if (body?["count"] != null) int.TryParse(body["count"]!.ToString(), out count);

            var list = new KbDocumentService().SearchDocuments(query, threshold, count <= 0 ? 5 : count);
            return ResponseResult<List<KbDocumentEntity>>.Success(list);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<KbDocumentEntity>>.Error(ex.Message);
        }
    }

    /// <summary>POST api/Wf2Ai/SaveKnowledgeBaseDocument · { Content, Metadata?, IndustryId? }</summary>
    [HttpPost]
    public ResponseResult<KbDocumentEntity> SaveKnowledgeBaseDocument([FromBody] KbDocumentEntity? entity)
    {
        try
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.Content))
                return ResponseResult<KbDocumentEntity>.Error("Content required");
            var saved = new KbDocumentService().SaveDocument(entity);
            return ResponseResult<KbDocumentEntity>.Success(saved, "saved");
        }
        catch (Exception ex)
        {
            return ResponseResult<KbDocumentEntity>.Error(ex.Message);
        }
    }

    /// <summary>POST/DELETE api/Wf2Ai/DeleteKnowledgeBaseDocument/{id}</summary>
    [HttpPost]
    [HttpDelete]
    public ResponseResult DeleteKnowledgeBaseDocument(long id)
    {
        try
        {
            if (id <= 0) return ResponseResult.Error("id required");
            var n = new KbDocumentService().DeleteDocumentById(id);
            return ResponseResult.Success($"deleted {n}");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    #endregion

    private static bool IsBlankOrMaskedApiKey(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return true;
        // 前端保存后用 • 掩码回显
        return apiKey.All(c => c == '•' || c == '*' || c == '·');
    }

    private static string ResolvePlainApiKey(string apiKey, string? apiUuid)
    {
        if (string.IsNullOrWhiteSpace(apiUuid))
            return apiKey;
        try
        {
            return ApiKeyCryptoHelper.Decrypt(apiKey, apiUuid);
        }
        catch
        {
            return apiKey;
        }
    }
}

/// <summary>测试模型连接请求体（对齐 sfdapi.Models.TestModelConnectionRequest）。</summary>
public class TestModelConnectionRequest
{
    public string? BaseUrl { get; set; }
    public string? ApiUUID { get; set; }
    public string? ApiKey { get; set; }
    public string? ModelProvider { get; set; }
}
