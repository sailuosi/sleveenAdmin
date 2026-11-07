using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using mooSQL.config;
using mooSQL.data;
using mooSQL.data.context;

namespace SleveenAdmin.Foundation.Data;

public static class MooSqlSetup
{
    public const int DefaultPosition = 0;

    public static IServiceCollection AddMooSql(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DBCoonfigOptions>(configuration.GetSection(DBCoonfigOptions.SectionName));
        services.AddSingleton<DBInsCash>(sp => CreateCash(sp));
        services.AddScoped(typeof(MooRepository<>));
        return services;
    }

    private static DBInsCash CreateCash(IServiceProvider sp)
    {
        var options = sp.GetRequiredService<IOptions<DBCoonfigOptions>>().Value;
        if (options.Connections.Count == 0)
            throw new InvalidOperationException("DBCoonfig:Connections 不能为空。");

        var env = sp.GetService<IHostEnvironment>();
        var cash = new DBClientBuilder().doBuild();

        foreach (var conn in options.Connections)
        {
            if (string.Equals(conn.DbType, "SQLITE", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(conn.DbType, "SQLite", StringComparison.OrdinalIgnoreCase))
            {
                conn.ConnectString = EnsureSqliteDataDirectory(conn.ConnectString, env);
            }
        }

        cash.addConfig(options.Connections);

        if (env != null)
            cash.getClient().useWatchor(new MooSqlQueryWatchor(env));

        DBCash.Bind(cash);
        return cash;
    }

    /// <summary>
    /// Resolves a relative SQLite Data Source under ContentRoot and creates the parent directory.
    /// </summary>
    internal static string EnsureSqliteDataDirectory(string connectString, IHostEnvironment? env)
    {
        if (string.IsNullOrWhiteSpace(connectString))
            return connectString;

        const string prefix = "Data Source=";
        var parts = connectString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var rebuilt = new List<string>(parts.Length);
        var changed = false;

        foreach (var part in parts)
        {
            if (!part.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                rebuilt.Add(part);
                continue;
            }

            var dataSource = part[prefix.Length..].Trim().Trim('"');
            if (string.IsNullOrEmpty(dataSource) ||
                dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase) ||
                Path.IsPathRooted(dataSource))
            {
                if (Path.IsPathRooted(dataSource))
                {
                    var parent = Path.GetDirectoryName(dataSource);
                    if (!string.IsNullOrEmpty(parent))
                        Directory.CreateDirectory(parent);
                }

                rebuilt.Add(part);
                continue;
            }

            var root = env?.ContentRootPath ?? Directory.GetCurrentDirectory();
            var absolute = Path.GetFullPath(Path.Combine(root, dataSource));
            var dir = Path.GetDirectoryName(absolute);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            rebuilt.Add(prefix + absolute);
            changed = true;
        }

        return changed ? string.Join(';', rebuilt) : connectString;
    }
}

internal sealed class MooSqlQueryWatchor : Watchor
{
    private readonly IHostEnvironment _host;

    public MooSqlQueryWatchor(IHostEnvironment host) => _host = host;

    public override string onAfterExecuteSetError(string oprationId, ExeContext context, Exception ex, string operation)
    {
        var msg = $"SQL执行错误: {context.cmd.cmdText} | {ex.Message}";
        Console.WriteLine(msg);
        try
        {
            var dir = Path.Combine(_host.ContentRootPath, "log", "moosql");
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, $"errSql{DateTime.Now:yyyyMMdd}.txt");
            File.AppendAllText(file, msg + Environment.NewLine);
        }
        catch
        {
            // ignore log failures
        }

        return msg;
    }
}
