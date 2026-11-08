using System.Text.Json.Serialization;
using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TableAppletCata, Caption = "门户组件分类")]
public class SysAppletCata
{
    [JsonPropertyName("sacId")]
    [SooColumn("Sac_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [JsonPropertyName("SYS_Created")]
    [SooColumn("Sac_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [JsonPropertyName("SYS_LAST_UPD")]
    [SooColumn("Sac_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [JsonPropertyName("SYS_Deleted")]
    [SooColumn("Sac_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [JsonPropertyName("sacPid")]
    [SooColumn("Sac_Pid", CanBeNull = true)]
    public Guid? Pid { get; set; }

    [JsonPropertyName("AC_Name")]
    [SooColumn("AC_Name", Length = 100, CanBeNull = true)]
    public string? Name { get; set; }

    [JsonPropertyName("AC_Code")]
    [SooColumn("AC_Code", Length = 50, CanBeNull = true)]
    public string? Code { get; set; }

    [JsonPropertyName("AC_Idx")]
    [SooColumn("AC_Idx", CanBeNull = true)]
    public int? Idx { get; set; }

    [JsonPropertyName("AC_Remark")]
    [SooColumn("AC_Remark", Length = 200, CanBeNull = true)]
    public string? Remark { get; set; }
}

