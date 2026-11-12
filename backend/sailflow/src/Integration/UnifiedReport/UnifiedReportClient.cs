using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SleveenAdmin.Saiflow.Integration.UnifiedReport;

/// <summary>统一用户中心 / 集成平台日程 HTTP 客户端（移植自 szkfpt UnifiedReportClient）。</summary>
public sealed class UnifiedReportClient
{
    private static readonly HashSet<string> SuccessCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "0", "200", "0000", "000000", "SUCCESS"
    };

    private readonly UnifiedReportOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UnifiedReportClient> _logger;
    private TokenHolder? _tokenCache;
    private readonly object _tokenLock = new();

    public UnifiedReportClient(
        IOptions<UnifiedReportOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<UnifiedReportClient> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task AddScheduleAsync(TaskInfoDto taskInfo, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(taskInfo);
        return AddSchedulesAsync(new[] { taskInfo }, ct);
    }

    public async Task AddSchedulesAsync(IReadOnlyList<TaskInfoDto> taskInfoList, CancellationToken ct = default)
    {
        if (taskInfoList == null || taskInfoList.Count == 0)
            throw new ArgumentException("taskInfoList 不能为空", nameof(taskInfoList));

        var body = Serialize(taskInfoList);
        var headers = await BuildScheduleHeadersAsync(_options.AddScheduleOperationCode, ct).ConfigureAwait(false);
        var url = BuildScheduleUrl("addSchedules");
        var response = await DoRequestAsync(url, HttpMethod.Post, body, headers, "添加待办", ct).ConfigureAwait(false);
        EnsureSuccess(response, "添加待办");
    }

    public async Task UpdateScheduleAsync(TaskInfoDto taskInfo, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(taskInfo);

        var body = Serialize(new[] { taskInfo });
        var headers = await BuildScheduleHeadersAsync(_options.UpdateScheduleOperationCode, ct).ConfigureAwait(false);
        var url = BuildScheduleUrl("updateSchedules");
        try
        {
            var response = await DoRequestAsync(url, HttpMethod.Post, body, headers, "更新待办", ct).ConfigureAwait(false);
            EnsureSuccess(response, "更新待办");
        }
        catch (UnifiedReportException ex) when (ex.Message.Contains("更新记录数为0", StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "[UnifiedReport] 更新待办无匹配记录（可能未推送过）, scheduleId={ScheduleId}, receiveUserCode={ReceiveUserCode}",
                taskInfo.AppScheduleId, taskInfo.ReceiveUserCode);
        }
    }

    public async Task DeleteScheduleAsync(TaskInfoDto taskInfo, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(taskInfo);

        var body = Serialize(new[] { taskInfo });
        var headers = await BuildScheduleHeadersAsync(_options.DeleteScheduleOperationCode, ct).ConfigureAwait(false);
        var url = BuildScheduleUrl("deleteSchedules");
        var response = await DoRequestAsync(url, HttpMethod.Post, body, headers, "删除待办", ct).ConfigureAwait(false);
        EnsureSuccess(response, "删除待办");
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        var cached = _tokenCache;
        if (cached != null && !cached.IsExpiringSoon(_options.TokenAheadSeconds))
            return cached.Token;

        lock (_tokenLock)
        {
            cached = _tokenCache;
            if (cached != null && !cached.IsExpiringSoon(_options.TokenAheadSeconds))
                return cached.Token;
        }

        var fresh = await RequestAccessTokenAsync(ct).ConfigureAwait(false);
        lock (_tokenLock)
        {
            _tokenCache = fresh;
        }
        return fresh.Token;
    }

    private async Task<TokenHolder> RequestAccessTokenAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
            throw new UnifiedReportException("UnifiedReport:BaseUrl 未配置");
        if (string.IsNullOrWhiteSpace(_options.ClientId))
            throw new UnifiedReportException("UnifiedReport:ClientId 未配置");
        if (string.IsNullOrWhiteSpace(_options.BasicAuthorization))
            throw new UnifiedReportException("UnifiedReport:BasicAuthorization 未配置");

        var url = BuildOperationUrl(_options.TokenOperationCode);
        if (!string.IsNullOrWhiteSpace(_options.GrantType))
            url += (url.Contains('?', StringComparison.Ordinal) ? "&" : "?") + "grant_type=" + Uri.EscapeDataString(_options.GrantType);

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ClientId"] = _options.ClientId,
            ["OperationCode"] = _options.TokenOperationCode,
            ["Authorization"] = _options.BasicAuthorization,
            ["Accept"] = "application/json",
        };

        var response = await DoRequestAsync(url, HttpMethod.Get, null, headers, "获取 token", ct).ConfigureAwait(false);
        var body = ParseBody(response);
        EnsureSuccess(body, "获取 token");

        var dataNode = body?["data"] ?? body;
        if (dataNode == null || dataNode.Type == JTokenType.Null)
            throw new UnifiedReportException("获取 token 返回数据为空");

        var token = ExtractText(dataNode, "accessToken", "access_token", "token");
        if (string.IsNullOrWhiteSpace(token))
            throw new UnifiedReportException("获取 token 返回中缺少 accessToken");

        var expireIn = ExtractLong(dataNode, 3600L, "expireIn", "expiresIn", "expires_in", "expire");
        var expireAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(expireIn, _options.TokenAheadSeconds + 1));
        return new TokenHolder(token, expireAt);
    }

    private async Task<Dictionary<string, string>> BuildScheduleHeadersAsync(string operationCode, CancellationToken ct)
    {
        var token = await GetAccessTokenAsync(ct).ConfigureAwait(false);
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ClientId"] = _options.ClientId,
            ["OperationCode"] = operationCode,
            ["Authorization"] = "Bearer " + token,
            ["Accept"] = "application/json",
            ["Content-Type"] = "application/json",
        };
    }

    private string BuildOperationUrl(string operationCode)
    {
        var baseUrl = (_options.BaseUrl ?? "").TrimEnd('/');
        var op = (operationCode ?? "").TrimStart('/');
        return $"{baseUrl}/{op}";
    }

    private string BuildScheduleUrl(string path)
    {
        var baseUrl = string.IsNullOrWhiteSpace(_options.ScheduleBaseUrl)
            ? (_options.BaseUrl ?? "")
            : _options.ScheduleBaseUrl;
        baseUrl = baseUrl.TrimEnd('/');
        var p = (path ?? "").TrimStart('/');
        return $"{baseUrl}/portal/schedule/{p}";
    }

    private async Task<string?> DoRequestAsync(
        string url,
        HttpMethod method,
        string? body,
        IReadOnlyDictionary<string, string> headers,
        string action,
        CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient(nameof(UnifiedReportClient));
        using var request = new HttpRequestMessage(method, url);
        foreach (var kv in headers)
        {
            if (string.Equals(kv.Key, "Content-Type", StringComparison.OrdinalIgnoreCase)
                || string.Equals(kv.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
        }

        if (headers.TryGetValue("Authorization", out var auth))
            request.Headers.TryAddWithoutValidation("Authorization", auth);

        if (body != null)
        {
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        try
        {
            using var response = await client.SendAsync(request, ct).ConfigureAwait(false);
            var text = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new UnifiedReportException(
                    $"{action}接口调用失败，HTTP 状态码：{(int)response.StatusCode}，body={Truncate(text)}");
            }
            return text;
        }
        catch (UnifiedReportException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UnifiedReport] {Action}接口请求异常，url={Url}", action, url);
            throw new UnifiedReportException($"{action}接口请求异常：{ex.Message}", ex);
        }
    }

    private static void EnsureSuccess(string? responseBody, string action) =>
        EnsureSuccess(ParseBody(responseBody), action);

    private static void EnsureSuccess(JToken? body, string action)
    {
        if (body == null || body.Type == JTokenType.Null)
            return;

        var codeNode = body["code"];
        if (codeNode != null && !SuccessCodes.Contains(codeNode.ToString()))
        {
            var msg = ExtractText(body, "msg", "message", "error") ?? codeNode.ToString();
            throw new UnifiedReportException(msg);
        }

        var successNode = body["success"];
        if (successNode != null && successNode.Type == JTokenType.Boolean && !successNode.Value<bool>())
        {
            var msg = ExtractText(body, "msg", "message", "error") ?? $"{action}失败";
            throw new UnifiedReportException(msg);
        }
    }

    private static JToken? ParseBody(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;
        try
        {
            return JToken.Parse(body);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ExtractText(JToken node, params string[] fieldNames)
    {
        foreach (var name in fieldNames)
        {
            var child = node[name];
            if (child != null && child.Type != JTokenType.Null)
                return child.ToString();
        }
        return null;
    }

    private static long ExtractLong(JToken node, long defaultValue, params string[] fieldNames)
    {
        foreach (var name in fieldNames)
        {
            var child = node[name];
            if (child != null && child.Type != JTokenType.Null && long.TryParse(child.ToString(), out var v))
                return v;
        }
        return defaultValue;
    }

    private static string Serialize(object payload) =>
        JsonConvert.SerializeObject(payload, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        });

    private static string Truncate(string? text, int max = 500)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= max ? text : text[..max];
    }

    private sealed record TokenHolder(string Token, DateTimeOffset ExpireAt)
    {
        public bool IsExpiringSoon(int aheadSeconds) =>
            DateTimeOffset.UtcNow.AddSeconds(Math.Max(aheadSeconds, 0)) >= ExpireAt;
    }
}

public sealed class UnifiedReportException : Exception
{
    public UnifiedReportException(string message) : base(message) { }
    public UnifiedReportException(string message, Exception inner) : base(message, inner) { }
}
