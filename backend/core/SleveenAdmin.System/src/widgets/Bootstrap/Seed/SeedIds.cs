namespace SleveenAdmin.System.Bootstrap.Seed;

/// <summary>
/// Seed 固定 Guid（含运行库现有菜单分组 Id，便于幂等补种）。
/// </summary>
public static class SeedIds
{
    public static readonly Guid SuperAdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid RootOrgId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    public static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000050");

    /// <summary>XX科技公司（根组织）下二级部门</summary>
    public static readonly Guid OrgDevDeptId = Guid.Parse("00000000-0000-0000-0000-0000000000A1");
    public static readonly Guid OrgCsDeptId = Guid.Parse("00000000-0000-0000-0000-0000000000A2");

    /// <summary>开发部演示人员（3）</summary>
    public static readonly Guid UserZhangSanId = Guid.Parse("00000000-0000-0000-0000-0000000000B1");
    public static readonly Guid UserLiSiId = Guid.Parse("00000000-0000-0000-0000-0000000000B2");
    public static readonly Guid UserWangWuId = Guid.Parse("00000000-0000-0000-0000-0000000000B3");

    /// <summary>客服部演示人员（5）</summary>
    public static readonly Guid UserZhaoLiuId = Guid.Parse("00000000-0000-0000-0000-0000000000B4");
    public static readonly Guid UserQianQiId = Guid.Parse("00000000-0000-0000-0000-0000000000B5");
    public static readonly Guid UserSunBaId = Guid.Parse("00000000-0000-0000-0000-0000000000B6");
    public static readonly Guid UserZhouJiuId = Guid.Parse("00000000-0000-0000-0000-0000000000B7");
    public static readonly Guid UserWuShiId = Guid.Parse("00000000-0000-0000-0000-0000000000B8");

    public static readonly Guid MenuHomeId = Guid.Parse("00000000-0000-0000-0000-000000000010");
    public static readonly Guid MenuAiId = Guid.Parse("00000000-0000-0000-0000-000000000011");

    /// <summary>设置（顶层分组，运行库）</summary>
    public static readonly Guid MenuSysConfigGroupId = Guid.Parse("01A1B186-32CF-7F94-BC3A-6A7C0878CFF7");

    public static readonly Guid MenuManageId = Guid.Parse("00000000-0000-0000-0000-000000000020");
    public static readonly Guid MenuManageUserId = Guid.Parse("00000000-0000-0000-0000-000000000021");
    public static readonly Guid MenuManageMenuId = Guid.Parse("00000000-0000-0000-0000-000000000023");
    public static readonly Guid MenuManageOrgId = Guid.Parse("00000000-0000-0000-0000-000000000024");
    public static readonly Guid MenuManageDictId = Guid.Parse("00000000-0000-0000-0000-000000000025");
    public static readonly Guid MenuManageConfigId = Guid.Parse("00000000-0000-0000-0000-000000000026");
    public static readonly Guid MenuManageAuthBagId = Guid.Parse("00000000-0000-0000-0000-000000000027");

    public static readonly Guid MenuInfraId = Guid.Parse("00000000-0000-0000-0000-000000000030");
    public static readonly Guid MenuInfraFileId = Guid.Parse("00000000-0000-0000-0000-000000000031");
    public static readonly Guid MenuInfraLogOpId = Guid.Parse("00000000-0000-0000-0000-000000000032");
    public static readonly Guid MenuInfraLogVisId = Guid.Parse("00000000-0000-0000-0000-000000000033");
    public static readonly Guid MenuInfraServerId = Guid.Parse("00000000-0000-0000-0000-000000000034");
    public static readonly Guid MenuInfraRedisId = Guid.Parse("00000000-0000-0000-0000-000000000035");

    public static readonly Guid MenuBtnUserAddId = Guid.Parse("00000000-0000-0000-0000-000000000040");
    public static readonly Guid MenuBtnUserEditId = Guid.Parse("00000000-0000-0000-0000-000000000041");
    public static readonly Guid MenuBtnUserDeleteId = Guid.Parse("00000000-0000-0000-0000-000000000042");
    public static readonly Guid MenuBtnAuthBagViewId = Guid.Parse("00000000-0000-0000-0000-000000000044");
    public static readonly Guid MenuBtnAuthBagEditId = Guid.Parse("00000000-0000-0000-0000-000000000045");

    public static readonly Guid MenuP1Id = Guid.Parse("00000000-0000-0000-0000-000000000060");
    public static readonly Guid MenuP1TenantId = Guid.Parse("00000000-0000-0000-0000-000000000061");
    public static readonly Guid MenuP1NoticeId = Guid.Parse("00000000-0000-0000-0000-000000000062");
    public static readonly Guid MenuP1NoticeInboxId = Guid.Parse("00000000-0000-0000-0000-000000000063");
    public static readonly Guid MenuP1OnlineUserId = Guid.Parse("00000000-0000-0000-0000-000000000064");
    public static readonly Guid MenuP1HangfireId = Guid.Parse("00000000-0000-0000-0000-000000000065");

    public static readonly Guid MenuPortalId = Guid.Parse("00000000-0000-0000-0000-000000000070");
    public static readonly Guid MenuPortalConfigId = Guid.Parse("00000000-0000-0000-0000-000000000071");
    public static readonly Guid MenuPortalPageId = Guid.Parse("00000000-0000-0000-0000-000000000072");
    public static readonly Guid MenuPortalAppletId = Guid.Parse("00000000-0000-0000-0000-000000000073");
    public static readonly Guid MenuPortalDesignId = Guid.Parse("00000000-0000-0000-0000-000000000074");

    public static readonly Guid MenuSailId = Guid.Parse("00000000-0000-0000-0000-000000000080");
    /// <summary>流程办理（运行库分组）</summary>
    public static readonly Guid MenuSailHandleId = Guid.Parse("01A1B18E-D449-750B-9536-46F87D35FBD4");
    /// <summary>流程管理（运行库分组）</summary>
    public static readonly Guid MenuSailManagersId = Guid.Parse("01A1B18F-6420-7728-906B-B783297D06AF");

    public static readonly Guid MenuSailTaskTodoId = Guid.Parse("00000000-0000-0000-0000-000000000081");
    public static readonly Guid MenuSailTaskDoneId = Guid.Parse("00000000-0000-0000-0000-000000000082");
    public static readonly Guid MenuSailTaskCopytoId = Guid.Parse("00000000-0000-0000-0000-000000000083");
    public static readonly Guid MenuSailProcessCreateId = Guid.Parse("00000000-0000-0000-0000-000000000084");
    public static readonly Guid MenuSailProcessMyId = Guid.Parse("00000000-0000-0000-0000-000000000085");
    public static readonly Guid MenuSailTaskManagerId = Guid.Parse("00000000-0000-0000-0000-000000000086");
    public static readonly Guid MenuSailProcessManagerId = Guid.Parse("00000000-0000-0000-0000-000000000087");
    public static readonly Guid MenuSailProcessesId = Guid.Parse("00000000-0000-0000-0000-000000000088");
    public static readonly Guid MenuSailFormsId = Guid.Parse("00000000-0000-0000-0000-000000000089");
    public static readonly Guid MenuSailRuleSetsId = Guid.Parse("00000000-0000-0000-0000-000000000090");
    public static readonly Guid MenuSailAiModelsId = Guid.Parse("00000000-0000-0000-0000-000000000091");
    public static readonly Guid MenuSailKnowledgeId = Guid.Parse("00000000-0000-0000-0000-000000000092");
    public static readonly Guid MenuSailOutboxId = Guid.Parse("00000000-0000-0000-0000-000000000093");
    public static readonly Guid MenuSailTemplatesId = Guid.Parse("00000000-0000-0000-0000-000000000094");
    public static readonly Guid MenuSailLeaveId = Guid.Parse("00000000-0000-0000-0000-000000000095");
    public static readonly Guid MenuSailCategoriesId = Guid.Parse("00000000-0000-0000-0000-000000000096");
    public static readonly Guid MenuSailReportId = Guid.Parse("00000000-0000-0000-0000-000000000097");
    public static readonly Guid MenuSailDesignerId = Guid.Parse("00000000-0000-0000-0000-000000000098");
    public static readonly Guid MenuSailFormEditorId = Guid.Parse("00000000-0000-0000-0000-000000000099");
    public static readonly Guid MenuSailProcessDetailId = Guid.Parse("00000000-0000-0000-0000-00000000009A");
    public static readonly Guid MenuSailProcessEditId = Guid.Parse("00000000-0000-0000-0000-00000000009B");
}
