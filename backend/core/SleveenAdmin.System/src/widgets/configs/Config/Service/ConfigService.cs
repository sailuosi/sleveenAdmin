using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.Foundation.Utils;
using SleveenAdmin.System.Config.DBLayer;
using SleveenAdmin.System.Config.Dtos;

namespace SleveenAdmin.System.Config.Service;

public sealed class ConfigService : IScoped
{
    private readonly MooRepository<SysConfig> _configs;

    public ConfigService(MooRepository<SysConfig> configs) => _configs = configs;

    public PagedResult<SysConfigDto> GetPage(PageQuery query)
    {
        var list = _configs.Repo.GetList();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            list = list.Where(c =>
                c.Code.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                (c.Remark != null && c.Remark.Contains(kw, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        return PageHelper.Apply(list.Select(ToDto).ToList(), query);
    }

    public SysConfigDto? GetById(Guid id)
    {
        var config = _configs.Repo.GetById(id);
        return config is null ? null : ToDto(config);
    }

    public SysConfigDto? GetByCode(string code)
    {
        var config = _configs.Repo.GetFirst(c => c.Code == code);
        return config is null ? null : ToDto(config);
    }

    public Guid Create(SysConfigInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Code))
            throw new ArgumentException("配置编码不能为空");
        if (_configs.Repo.GetFirst(c => c.Code == input.Code) is not null)
            throw new InvalidOperationException("配置编码已存在");

        var id = Guid.CreateVersion7();
        _configs.Repo.Insert(new SysConfig
        {
            Id = id,
            Code = input.Code.Trim(),
            Value = input.Value,
            Remark = input.Remark
        });
        return id;
    }

    public void Update(SysConfigInput input)
    {
        if (input.Id is null || input.Id == Guid.Empty)
            throw new ArgumentException("Id 不能为空");

        var config = _configs.Repo.GetById(input.Id.Value)
                     ?? throw new InvalidOperationException("配置不存在");

        if (!string.IsNullOrWhiteSpace(input.Code) && input.Code != config.Code)
        {
            if (_configs.Repo.GetFirst(c => c.Code == input.Code && c.Id != config.Id) is not null)
                throw new InvalidOperationException("配置编码已存在");
            config.Code = input.Code.Trim();
        }

        config.Value = input.Value;
        config.Remark = input.Remark;
        _configs.Repo.Update(config);
    }

    public void Delete(Guid id)
    {
        if (!_configs.Repo.DeleteById(id))
            throw new InvalidOperationException("配置不存在");
    }

    private static SysConfigDto ToDto(SysConfig config) => new()
    {
        Id = config.Id,
        Code = config.Code,
        Value = config.Value,
        Remark = config.Remark
    };
}
