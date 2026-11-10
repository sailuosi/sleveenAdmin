using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.Foundation.Utils;
using SleveenAdmin.System.Dict.DBLayer;
using SleveenAdmin.System.Dict.Dtos;

namespace SleveenAdmin.System.Dict.Service;

[Scoped]
public sealed class DictService {
    private readonly MooRepository<SysDictType> _types;
    private readonly MooRepository<SysDictData> _data;

    public DictService(MooRepository<SysDictType> types, MooRepository<SysDictData> data)
    {
        _types = types;
        _data = data;
    }

    public PagedResult<SysDictTypeDto> GetTypePage(PageQuery query)
    {
        var list = _types.Repo.GetList();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            list = list.Where(t =>
                t.Code.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                t.Name.Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return PageHelper.Apply(list.Select(ToTypeDto).ToList(), query);
    }

    public SysDictTypeDto? GetTypeById(Guid id)
    {
        var type = _types.Repo.GetById(id);
        return type is null ? null : ToTypeDto(type);
    }

    public Guid CreateType(SysDictTypeInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Code) || string.IsNullOrWhiteSpace(input.Name))
            throw new ArgumentException("字典编码和名称不能为空");
        if (_types.Repo.GetFirst(t => t.Code == input.Code) is not null)
            throw new InvalidOperationException("字典编码已存在");

        var id = Guid.CreateVersion7();
        _types.Repo.Insert(new SysDictType
        {
            Id = id,
            Code = input.Code.Trim(),
            Name = input.Name.Trim(),
            Remark = input.Remark
        });
        return id;
    }

    public void UpdateType(SysDictTypeInput input)
    {
        if (input.Id is null || input.Id == Guid.Empty)
            throw new ArgumentException("Id 不能为空");

        var type = _types.Repo.GetById(input.Id.Value)
                   ?? throw new InvalidOperationException("字典类型不存在");

        if (!string.IsNullOrWhiteSpace(input.Code) && input.Code != type.Code)
        {
            if (_types.Repo.GetFirst(t => t.Code == input.Code && t.Id != type.Id) is not null)
                throw new InvalidOperationException("字典编码已存在");
            type.Code = input.Code.Trim();
        }

        type.Name = input.Name.Trim();
        type.Remark = input.Remark;
        _types.Repo.Update(type);
    }

    public void DeleteType(Guid id)
    {
        var data = _data.Repo.GetList(d => d.TypeId == id);
        foreach (var item in data)
            _data.Repo.Delete(item);

        if (!_types.Repo.DeleteById(id))
            throw new InvalidOperationException("字典类型不存在");
    }

    public PagedResult<SysDictDataDto> GetDataPage(Guid typeId, PageQuery query)
    {
        var list = _data.Repo.GetList(d => d.TypeId == typeId).OrderBy(d => d.OrderNo).ToList();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            list = list.Where(d =>
                d.Label.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                d.Value.Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return PageHelper.Apply(list.Select(ToDataDto).ToList(), query);
    }

    public SysDictDataDto? GetDataById(Guid id)
    {
        var item = _data.Repo.GetById(id);
        return item is null ? null : ToDataDto(item);
    }

    public IReadOnlyList<SysDictDataDto> GetDataByCode(string code)
    {
        var type = _types.Repo.GetFirst(t => t.Code == code);
        if (type is null) return [];
        return _data.Repo.GetList(d => d.TypeId == type.Id).OrderBy(d => d.OrderNo).Select(ToDataDto).ToList();
    }

    public Guid CreateData(SysDictDataInput input)
    {
        if (_types.Repo.GetById(input.TypeId) is null)
            throw new InvalidOperationException("字典类型不存在");
        if (string.IsNullOrWhiteSpace(input.Label) || string.IsNullOrWhiteSpace(input.Value))
            throw new ArgumentException("字典标签和值不能为空");

        var id = Guid.CreateVersion7();
        _data.Repo.Insert(new SysDictData
        {
            Id = id,
            TypeId = input.TypeId,
            Label = input.Label.Trim(),
            Value = input.Value.Trim(),
            OrderNo = input.OrderNo
        });
        return id;
    }

    public void UpdateData(SysDictDataInput input)
    {
        if (input.Id is null || input.Id == Guid.Empty)
            throw new ArgumentException("Id 不能为空");

        var item = _data.Repo.GetById(input.Id.Value)
                   ?? throw new InvalidOperationException("字典数据不存在");

        item.TypeId = input.TypeId;
        item.Label = input.Label.Trim();
        item.Value = input.Value.Trim();
        item.OrderNo = input.OrderNo;
        _data.Repo.Update(item);
    }

    public void DeleteData(Guid id)
    {
        if (!_data.Repo.DeleteById(id))
            throw new InvalidOperationException("字典数据不存在");
    }

    private static SysDictTypeDto ToTypeDto(SysDictType type) => new()
    {
        Id = type.Id,
        Code = type.Code,
        Name = type.Name,
        Remark = type.Remark
    };

    private static SysDictDataDto ToDataDto(SysDictData data) => new()
    {
        Id = data.Id,
        TypeId = data.TypeId,
        Label = data.Label,
        Value = data.Value,
        OrderNo = data.OrderNo
    };
}
