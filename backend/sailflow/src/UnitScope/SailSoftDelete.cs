namespace SleveenAdmin.Sailflow.UnitScope;

/// <summary>设计资源逻辑删除：Deleted=1 表示已删（对齐 minehub NFlowSoftDelete）。</summary>
public static class SailSoftDelete
{
    public static bool IsDeleted(int deleted) => deleted == 1;

    public static bool IsAlive(int deleted) => deleted != 1;

    public static void MarkDeleted(Action<int> setDeleted, Action<DateTime?> setDeleteTime)
    {
        setDeleted(1);
        setDeleteTime(DateTime.Now);
    }
}
