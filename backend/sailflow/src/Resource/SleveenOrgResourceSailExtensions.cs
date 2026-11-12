using sailflow.engine.Client;
using sailflow.engine.Hosting;
using sailflow.hub.Resource;
using SleveenAdmin.Foundation.Data;

namespace SleveenAdmin.Saiflow.Resource;

/// <summary>
/// 宿主组织人员能力包：具化 <see cref="IDeptService"/> / <see cref="IResourceService"/> 并注入 Locator。
/// 引擎侧仅有契约；本扩展完成 SysOrg/SysUser/AuthVisitorBag 实现注册。
/// </summary>
public static class SleveenOrgResourceSailExtensions
{
    const string FeatureName = "host.sleveen.org-resource";

    /// <summary>
    /// Builder：ConfigureClient 注册宿主组织人员实现（契约具化 → Locator）。
    /// </summary>
    public static SailClientBuilder UseSleveenOrgResource(this SailClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.UseSleveenOrgResource();
        return builder;
    }

    /// <summary>Collection 管道注册。</summary>
    public static ISailCollection UseSleveenOrgResource(this ISailCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddFeature(new SailFeatureDescriptor
        {
            Name = FeatureName,
            Phase = SailFeaturePhase.ConfigureClient,
            Order = 110,
            ConfigureClient = (_, coll) => RegisterHostOrgResource(coll)
        });
    }

    /// <summary>已构建 Client 上补挂（测试/热补）。</summary>
    public static SailClient useSleveenOrgResource(this SailClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        RegisterHostOrgResource(null);
        return client;
    }

    static void RegisterHostOrgResource(ISailCollection? coll)
    {
        var dept = new SleveenDeptService { MasterDbPosition = MooSqlSetup.DefaultPosition };
        DeptServiceLocator.Register(dept);

        var resource = new SleveenResourceService(dept, MooSqlSetup.DefaultPosition);
        ResourceServiceLocator.Register(resource);
        if (coll != null)
            coll.Properties[SailBuilderKeys.ResourceService] = resource;
    }
}
