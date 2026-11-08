using System.Text.Json.Serialization;
using mooSQL.data;
using SleveenAdmin.Business.Portal.DBLayer;

namespace SleveenAdmin.Business.Portal.Service;

public class BizSysPortalRow : SysPortal
{
    [JsonPropertyName("PP_Title")]
    public string? PP_Title { get; set; }

    [JsonPropertyName("OrgName")]
    public string? OrgName { get; set; }
}

public class BizSysPortPageRow : SysPortPage
{
}

public class BizSysPortCellRow : SysPortCell
{
}

public class BizSysPortCellLinkRow : SysPortCellLink
{
}

public class BizSysAppletRow : SysApplet
{
}

public class BizSysAppletCataRow : SysAppletCata
{
}
