using Microsoft.AspNetCore.Http;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.Foundation.Storage;
using SleveenAdmin.Foundation.Utils;
using SleveenAdmin.System.File.DBLayer;
using SleveenAdmin.System.File.Dtos;

namespace SleveenAdmin.System.File.Service;

public sealed class FileService : IScoped
{
    private readonly MooRepository<SysFile> _files;
    private readonly LocalFileStorage _storage;

    public FileService(MooRepository<SysFile> files, LocalFileStorage storage)
    {
        _files = files;
        _storage = storage;
    }

    public PagedResult<SysFileDto> GetPage(PageQuery query)
    {
        var list = _files.Repo.GetList()
            .OrderByDescending(f => f.CreateTime)
            .ToList();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            list = list.Where(f =>
                f.OriginalName.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                f.StoredName.Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return PageHelper.Apply(list.Select(ToDto).ToList(), query);
    }

    public SysFileDto? GetById(Guid id)
    {
        var file = _files.Repo.GetById(id);
        return file is null ? null : ToDto(file);
    }

    public async Task<SysFileDto> UploadAsync(IFormFile file, Guid? userId, CancellationToken cancellationToken = default)
    {
        var (relativePath, storedName) = await _storage.SaveAsync(file, cancellationToken);
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var id = Guid.CreateVersion7();
        var entity = new SysFile
        {
            Id = id,
            OriginalName = file.FileName,
            StoredName = storedName,
            Extension = ext,
            Size = file.Length,
            ContentType = file.ContentType,
            Path = relativePath,
            Url = _storage.BuildPublicUrl(relativePath),
            CreateUserId = userId,
            CreateTime = DateTime.Now
        };
        _files.Repo.Insert(entity);
        return ToDto(entity);
    }

    public (string fullPath, string contentType, string downloadName)? GetDownload(Guid id)
    {
        var file = _files.Repo.GetById(id)
                     ?? throw new InvalidOperationException("文件不存在");

        var fullPath = _storage.GetFullPath(file.Path);
        if (!global::System.IO.File.Exists(fullPath))
            throw new InvalidOperationException("物理文件不存在");

        return (fullPath, file.ContentType ?? "application/octet-stream", file.OriginalName);
    }

    public void Delete(Guid id)
    {
        var file = _files.Repo.GetById(id)
                     ?? throw new InvalidOperationException("文件不存在");

        _storage.TryDelete(file.Path);
        if (!_files.Repo.DeleteById(id))
            throw new InvalidOperationException("文件删除失败");
    }

    private static SysFileDto ToDto(SysFile file) => new()
    {
        Id = file.Id,
        OriginalName = file.OriginalName,
        StoredName = file.StoredName,
        Extension = file.Extension,
        Size = file.Size,
        ContentType = file.ContentType,
        Path = file.Path,
        Url = file.Url,
        CreateUserId = file.CreateUserId,
        CreateTime = file.CreateTime
    };
}
