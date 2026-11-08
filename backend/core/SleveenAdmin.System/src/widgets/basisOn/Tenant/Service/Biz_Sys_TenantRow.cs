using mooSQL.data;
using SleveenAdmin.System.Tenant.DBLayer;

namespace SleveenAdmin.System.Tenant.Service;

[SooTable(Name = "SysTenant", Alias = "mtb", Type = DBTableType.Select)]
public class BizSysTenantRow : SysTenant
{
}
