using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Models;

namespace SleveenAdmin.Business.Portal.Dtos;

public sealed class PortalPageQuery : PageQuery
{
    /// <summary>兼容 sysext 列表 pageNum</summary>
    [FromQuery(Name = "pageNum")]
    public int? PageNum
    {
        get => Page;
        set { if (value is > 0) Page = value.Value; }
    }

    public Guid? SpOrgId { get; set; }
    public Guid? SpPortPageId { get; set; }

    [FromQuery(Name = "P_Name")]
    public string? PName { get; set; }

    [FromQuery(Name = "P_Key")]
    public string? PKey { get; set; }

    [FromQuery(Name = "P_Task")]
    public string? PTask { get; set; }

    [FromQuery(Name = "P_IsOn")]
    public bool? PIsOn { get; set; }

    public Guid? SpPortalId { get; set; }

    /// <summary>区块下控件列表：前端传 spPortCellId</summary>
    public Guid? SpPortCellId { get; set; }

    public Guid? SaAppletCataId { get; set; }
}
