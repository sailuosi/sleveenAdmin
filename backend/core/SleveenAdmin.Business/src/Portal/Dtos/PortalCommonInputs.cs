using System.Text.Json.Nodes;

namespace SleveenAdmin.Business.Portal.Dtos;

public sealed class StringValueInput
{
    public string? Value { get; set; }
}

public sealed class GuidValueInput
{
    public Guid Value { get; set; }
}

public sealed class SaveCellsInput
{
    public Guid PortalId { get; set; }
    public JsonArray? Cells { get; set; }
}

public sealed class SaveAppletsInput
{
    public Guid PortalId { get; set; }
    public JsonArray? Applets { get; set; }
}
