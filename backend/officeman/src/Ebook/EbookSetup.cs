using Microsoft.Extensions.DependencyInjection;
using SleveenAdmin.Officeman;

namespace SleveenAdmin.Officeman.Ebook;

public static class EbookSetup
{
    public static IServiceCollection AddSleveenAdminOfficeman(this IServiceCollection services)
    {
        services.AddSleveenAdminOfficemanServices();
        return services;
    }
}
