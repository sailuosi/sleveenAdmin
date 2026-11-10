using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Options;

namespace SleveenAdmin.Foundation.Storage;

[Singleton]
public sealed class LocalFileStorage
{
    private readonly FileStorageOptions _options;
    private readonly string _rootPath;

    public LocalFileStorage(IOptions<FileStorageOptions> options, IHostEnvironment env)
    {
        _options = options.Value;
        _rootPath = Path.IsPathRooted(_options.RootPath)
            ? _options.RootPath
            : Path.Combine(env.ContentRootPath, _options.RootPath);
    }

    public string RootPath => _rootPath;

    public string PublicPathPrefix => _options.PublicPathPrefix.TrimEnd('/');

    public void EnsureRootExists() => Directory.CreateDirectory(_rootPath);

    public void ValidateFile(IFormFile file)
    {
        if (file.Length == 0)
            throw new ArgumentException("文件不能为空");

        var maxBytes = (long)_options.MaxSizeMb * 1024 * 1024;
        if (file.Length > maxBytes)
            throw new InvalidOperationException($"文件大小不能超过 {_options.MaxSizeMb} MB");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (_options.AllowedExtensions.Length > 0 &&
            !_options.AllowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"不允许的文件类型: {ext}");
    }

    public async Task<(string relativePath, string storedName)> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        ValidateFile(file);
        EnsureRootExists();

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var storedName = $"{Guid.CreateVersion7():N}{ext}";
        var subDir = Path.Combine(DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"));
        var dir = Path.Combine(_rootPath, subDir);
        Directory.CreateDirectory(dir);

        var fullPath = Path.Combine(dir, storedName);
        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, cancellationToken);

        var relativePath = Path.Combine(subDir, storedName).Replace('\\', '/');
        return (relativePath, storedName);
    }

    public string BuildPublicUrl(string relativePath) =>
        $"{PublicPathPrefix}/{relativePath.Replace('\\', '/')}";

    public string GetFullPath(string relativePath) =>
        Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

    public bool TryDelete(string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
            return false;
        File.Delete(fullPath);
        return true;
    }
}
