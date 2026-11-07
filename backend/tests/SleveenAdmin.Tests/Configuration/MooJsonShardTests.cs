using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SleveenAdmin.Foundation.Configuration;

namespace SleveenAdmin.Tests.Configuration;

public class MooJsonShardTests
{
    [Fact]
    public void SplitConfigFileNameToGroups_groups_base_and_environment_files()
    {
        var files = new[]
        {
            @"C:\cfg\Cache.json",
            @"C:\cfg\Cache.Development.json",
            @"C:\cfg\A.B.Development.json"
        };

        var groups = MooJsonShardExtensions.SplitConfigFileNameToGroups(files)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        Assert.True(groups.ContainsKey("Cache"));
        Assert.Equal(2, groups["Cache"].Count);
        Assert.True(groups.ContainsKey("A.B"));
        Assert.Single(groups["A.B"]);
    }

    [Fact]
    public void PreferContentRootFile_prefers_content_root_copy()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), "moo-shard-content-" + Guid.NewGuid().ToString("N"));
        var baseDir = Path.Combine(Path.GetTempPath(), "moo-shard-base-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(contentRoot);
        Directory.CreateDirectory(baseDir);

        try
        {
            var contentFile = Path.Combine(contentRoot, "Jwt.json");
            var baseFile = Path.Combine(baseDir, "Jwt.json");
            File.WriteAllText(contentFile, "{}");
            File.WriteAllText(baseFile, "{}");

            var preferred = MooJsonShardExtensions.PreferContentRootFile(
                [baseFile, contentFile], contentRoot, baseDir);

            Assert.Equal(Path.GetFullPath(contentFile), Path.GetFullPath(preferred));
        }
        finally
        {
            TryDelete(contentRoot);
            TryDelete(baseDir);
        }
    }

    [Fact]
    public void AddMooJsonShards_merges_shards_and_environment_overlay()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), "moo-shard-root-" + Guid.NewGuid().ToString("N"));
        var configDir = Path.Combine(contentRoot, "Configuration");
        Directory.CreateDirectory(configDir);

        try
        {
            File.WriteAllText(Path.Combine(contentRoot, "appsettings.json"), """
                {
                  "ConfigurationScanDirectories": [ "Configuration" ]
                }
                """);

            File.WriteAllText(Path.Combine(configDir, "Jwt.json"), """
                {
                  "Jwt": {
                    "Issuer": "from-shard",
                    "AccessTokenMinutes": 30
                  }
                }
                """);

            File.WriteAllText(Path.Combine(configDir, "Jwt.Development.json"), """
                {
                  "Jwt": {
                    "AccessTokenMinutes": 90
                  }
                }
                """);

            File.WriteAllText(Path.Combine(configDir, "secret.local.json"), """
                {
                  "Jwt": { "Issuer": "should-ignore" }
                }
                """);

            var env = new TestHostEnvironment
            {
                EnvironmentName = "Development",
                ContentRootPath = contentRoot,
                ApplicationName = "tests",
                ContentRootFileProvider = new PhysicalFileProvider(contentRoot)
            };

            var builder = new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddJsonFile("appsettings.json", optional: false);

            builder.AddMooJsonShards(env, o =>
            {
                o.IgnoreFiles.Add("*.local.json");
                o.ReloadOnChange = false;
            });

            var config = builder.Build();

            Assert.Equal("from-shard", config["Jwt:Issuer"]);
            Assert.Equal("90", config["Jwt:AccessTokenMinutes"]);
        }
        finally
        {
            TryDelete(contentRoot);
        }
    }

    [Fact]
    public void AddMooJsonShards_excludes_appsettings_from_shard_scan()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), "moo-shard-apps-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(contentRoot);

        try
        {
            File.WriteAllText(Path.Combine(contentRoot, "appsettings.json"), """
                { "ConfigurationScanDirectories": [ "" ], "Marker": "host" }
                """);
            File.WriteAllText(Path.Combine(contentRoot, "Feature.json"), """
                { "Feature": { "Enabled": true } }
                """);

            var env = new TestHostEnvironment
            {
                EnvironmentName = "Production",
                ContentRootPath = contentRoot,
                ApplicationName = "tests",
                ContentRootFileProvider = new PhysicalFileProvider(contentRoot)
            };

            // 将 BaseDirectory 与 ContentRoot 隔离：仅依赖 ContentRoot 扫描
            var builder = new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddJsonFile("appsettings.json", optional: false);

            builder.AddMooJsonShards(env, o => o.ReloadOnChange = false);
            var config = builder.Build();

            Assert.Equal("host", config["Marker"]);
            Assert.Equal("True", config["Feature:Enabled"]);
        }
        finally
        {
            TryDelete(contentRoot);
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
            // best-effort cleanup
        }
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "tests";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
