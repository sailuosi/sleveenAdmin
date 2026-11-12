using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

namespace SleveenAdmin.Sailflow.Utility;

public sealed class SailButtonSettingDto
{
    public string Id { get; set; } = "";
    public bool Enable { get; set; } = true;
    public string DisplayName { get; set; } = "";
    public string? Engine { get; set; }
}

/// <summary>Sail OA 办理按钮码表 + BPMN sf:ButtonsSetting 解析。</summary>
public static class SailButtonsCatalog
{
    public static IReadOnlyList<SailButtonSettingDto> Defaults { get; } =
    [
        new() { Id = "Approve", DisplayName = "通过", Enable = true, Engine = "RunProcess" },
        new() { Id = "Reject", DisplayName = "驳回", Enable = true, Engine = "RejectProcess" },
        new() { Id = "SendBack", DisplayName = "退回", Enable = true, Engine = "SendBackProcess" },
        new() { Id = "SignForward", DisplayName = "加签", Enable = true, Engine = "SignForwardProcess" },
        new() { Id = "Transfer", DisplayName = "转办", Enable = true, Engine = "TransferProcess" },
        new() { Id = "Copy", DisplayName = "抄送", Enable = true, Engine = "WfCopy/Create" },
        new() { Id = "Withdraw", DisplayName = "撤回", Enable = true, Engine = "WithdrawProcess" },
        new() { Id = "Resend", DisplayName = "重发", Enable = true, Engine = "ResendProcess" }, // §G：有 BackSrc 时办理台文案「返送退回人」
        new() { Id = "Jump", DisplayName = "跳转", Enable = true, Engine = "JumpProcess" },
        new() { Id = "Close", DisplayName = "关闭", Enable = true, Engine = "CloseProcess" },
        // §B：默认关；旧图 merge 不出现启用态
        new() { Id = "Terminate", DisplayName = "终止", Enable = false, Engine = "TerminateProcess" },
        new() { Id = "Discard", DisplayName = "作废", Enable = false, Engine = "DiscardProcess" },
        new() { Id = "Revise", DisplayName = "修订", Enable = false, Engine = "ReviseProcess" },
        new() { Id = "Reverse", DisplayName = "反转", Enable = false, Engine = "ReverseProcess" }
    ];

    public static List<SailButtonSettingDto> Merge(IEnumerable<SailButtonSettingDto>? raw)
    {
        var map = (raw ?? []).Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .ToDictionary(x => x.Id, x => x, StringComparer.OrdinalIgnoreCase);
        return Defaults.Select(d =>
        {
            if (!map.TryGetValue(d.Id, out var hit))
                return Clone(d);
            return new SailButtonSettingDto
            {
                Id = d.Id,
                Enable = hit.Enable,
                DisplayName = string.IsNullOrWhiteSpace(hit.DisplayName) ? d.DisplayName : hit.DisplayName.Trim(),
                Engine = d.Engine
            };
        }).ToList();
    }

    public static List<SailButtonSettingDto> ParseForActivity(string? xml, string? activityId)
    {
        if (string.IsNullOrWhiteSpace(xml) || string.IsNullOrWhiteSpace(activityId))
            return Merge(null);

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("bpmn", "http://www.omg.org/spec/BPMN/20100524/MODEL");
            nsmgr.AddNamespace("sf", "http://www.saiflow.com/schema/sf");

            XmlNode? activity =
                doc.SelectSingleNode($"//*[@id='{EscapeXPath(activityId)}']") ??
                doc.SelectSingleNode($"//bpmn:userTask[@id='{EscapeXPath(activityId)}']", nsmgr) ??
                doc.SelectSingleNode($"//bpmn:task[@id='{EscapeXPath(activityId)}']", nsmgr);

            if (activity == null)
                return Merge(null);

            XmlNode? buttons =
                activity.SelectSingleNode(".//*[local-name()='ButtonsSetting']");
            if (buttons == null)
                return Merge(null);

            var json = buttons.Attributes?["json"]?.Value
                       ?? buttons.Attributes?["sf:json"]?.Value
                       ?? buttons.InnerText?.Trim();
            return ParseJson(json);
        }
        catch
        {
            return ParseJsonRegex(xml!, activityId!);
        }
    }

    public static List<SailButtonSettingDto> ParseJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Merge(null);
        try
        {
            var list = JsonSerializer.Deserialize<List<SailButtonSettingDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Merge(list);
        }
        catch
        {
            return Merge(null);
        }
    }

    static List<SailButtonSettingDto> ParseJsonRegex(string xml, string activityId)
    {
        var idEsc = Regex.Escape(activityId);
        var m = Regex.Match(
            xml,
            $@"id=[""']{idEsc}[""'][\s\S]{{0,4000}}?ButtonsSetting[^>]*json=[""']([^""']+)[""']",
            RegexOptions.IgnoreCase);
        if (!m.Success)
            return Merge(null);
        var raw = m.Groups[1].Value.Replace("&quot;", "\"").Replace("&amp;", "&");
        return ParseJson(raw);
    }

    static string EscapeXPath(string id) => id.Replace("'", "&apos;");

    static SailButtonSettingDto Clone(SailButtonSettingDto d) => new()
    {
        Id = d.Id,
        Enable = d.Enable,
        DisplayName = d.DisplayName,
        Engine = d.Engine
    };
}
