using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.Foundation.Tenant;
using SleveenAdmin.Foundation.Utils;
using SleveenAdmin.System.AuthBag.DBLayer;
using SleveenAdmin.System.AuthBag.Dtos;
using SleveenAdmin.System.Author;
using SleveenAdmin.System.Author.AuthBag;
using SleveenAdmin.System.Menu.DBLayer;
using mooSQL.data;
using mooSQL.utils;
using mooSQL.data;

namespace SleveenAdmin.System.AuthBag.Service;

[Scoped]
public sealed class AuthBagService {
    private readonly MooRepository<AuthDutyItem> _dutyItems;
    private readonly MooRepository<AuthGoodsBag> _goodsBags;
    private readonly MooRepository<AuthGoodsItem> _goodsItems;
    private readonly MooRepository<AuthVisitorBag> _visitorBags;
    private readonly MooRepository<AuthVisitorItem> _visitorItems;
    private readonly MooRepository<AuthWordBag> _wordBags;
    private readonly MooRepository<AuthWordItem> _wordItems;
    private readonly MooRepository<AuthWordDefine> _wordDefines;
    private readonly MooRepository<SysMenu> _menus;
    private readonly AuthCacheService _cache;
    private readonly ITenantContext _tenant;

    public AuthBagService(
        MooRepository<AuthDutyItem> dutyItems,
        MooRepository<AuthGoodsBag> goodsBags,
        MooRepository<AuthGoodsItem> goodsItems,
        MooRepository<AuthVisitorBag> visitorBags,
        MooRepository<AuthVisitorItem> visitorItems,
        MooRepository<AuthWordBag> wordBags,
        MooRepository<AuthWordItem> wordItems,
        MooRepository<AuthWordDefine> wordDefines,
        MooRepository<SysMenu> menus,
        AuthCacheService cache,
        ITenantContext tenant)
    {
        _dutyItems = dutyItems;
        _goodsBags = goodsBags;
        _goodsItems = goodsItems;
        _visitorBags = visitorBags;
        _visitorItems = visitorItems;
        _wordBags = wordBags;
        _wordItems = wordItems;
        _wordDefines = wordDefines;
        _menus = menus;
        _cache = cache;
        _tenant = tenant;
    }

    #region DutyItem

    public PagedResult<AuthDutyItemDto> GetDutyPage(AuthBagPageQuery query)
    {
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        var dt = kit.select(
                "a.*, vb.Avb_Name as VisitorBagName, wb.Awb_Name as WordBagName, gb.Agb_Name as GoodsBagName")
            .from("AuthDutyItem a")
            .join("left join AuthVisitorBag vb on a.Adi_VisitorBagId=vb.Avb_Id")
            .join("left join AuthWordBag wb on a.Adi_WordBagId=wb.Awb_Id")
            .join("left join AuthGoodsBag gb on a.Adi_GoodsBagId=gb.Agb_Id")
            .where("(a.Adi_IsDeleted=0 or a.Adi_IsDeleted is null)")
            .whereLike("a.Adi_Name", query.Keyword)
            .whereLike("a.Adi_Code", query.Keyword)
            .whereLike("a.Adi_Note", query.Keyword)
            .setPage(query.PageSize, query.Page)
            .orderBy("a.Adi_Idx asc, a.Adi_CreateTime desc, a.Adi_Id asc")
            .query();
        var total = kit.count();
        var rows = dt.Rows.Cast<global::System.Data.DataRow>().Select(MapDuty).ToList();
        return PagedResult<AuthDutyItemDto>.Create(rows, query.Page, query.PageSize, total);
    }

    public AuthDutyItemDto? GetDuty(Guid id)
    {
        var row = _dutyItems.Repo.GetById(id);
        return row is null || row.IsDeleted ? null : MapDuty(row);
    }

    public Guid SaveDuty(AuthDutyItemDto input, IAuthUserContext user)
    {
        var now = DateTime.Now;
        if (input.Id == Guid.Empty)
        {
            var entity = new AuthDutyItem
            {
                Id = Guid.CreateVersion7(),
                Name = input.Name,
                Code = input.Code,
                Note = input.Note,
                Remark = input.Remark,
                Idx = input.Idx,
                VisitorBagId = input.VisitorBagId,
                WordBagId = input.WordBagId,
                GoodsBagId = input.GoodsBagId,
                TenantId = _tenant.TenantId,
                CreateTime = now,
                UpdateTime = now,
                CreateUserId = user.UserId,
                UpdateUserId = user.UserId
            };
            _dutyItems.Repo.Insert(entity);
            return entity.Id;
        }

        var existing = _dutyItems.Repo.GetById(input.Id)
                     ?? throw new InvalidOperationException("权限条目不存在");
        existing.Name = input.Name;
        existing.Code = input.Code;
        existing.Note = input.Note;
        existing.Remark = input.Remark;
        existing.Idx = input.Idx;
        existing.VisitorBagId = input.VisitorBagId;
        existing.WordBagId = input.WordBagId;
        existing.GoodsBagId = input.GoodsBagId;
        existing.UpdateTime = now;
        existing.UpdateUserId = user.UserId;
        _dutyItems.Repo.Update(existing);
        return existing.Id;
    }

    public void DeleteDuty(Guid id)
    {
        var existing = _dutyItems.Repo.GetById(id)
                     ?? throw new InvalidOperationException("权限条目不存在");
        existing.IsDeleted = true;
        existing.UpdateTime = DateTime.Now;
        _dutyItems.Repo.Update(existing);
    }

    public int CleanCache(CleanCacheInput input)
    {
        var count = 0;
        if (input.Mode == "1")
        {
            count += _cache.RemoveByPrefix("userVisitorBags:");
            count += _cache.RemoveByPrefix("orgCode:");
            count += _cache.RemoveByPrefix("orgVisitorBags:");
            count += _cache.RemoveByPrefix("manDivision:");
        }
        else if (input.Mode == "2")
        {
            count += _cache.RemoveByPrefix("loadRole:");
            count += _cache.RemoveByPrefix("userDutys:");
            count += _cache.RemoveByPrefix("userVisitorBags:");
        }

        return count;
    }

    public object LoadHisVisitors(HisAuthInput input)
    {
        var nowBags = DutyLoader.loadHisVisitorBag(input.Account, false);
        var cachedBags = DutyLoader.loadHisVisitorBag(input.Account, true);
        var kit = DBCash.useSQL(MooSqlSetup.DefaultPosition);
        return new
        {
            now = kit.select("*").from("AuthVisitorBag g").whereIn("g.Avb_Id", nowBags).query(),
            cached = kit.clear().select("*").from("AuthVisitorBag g").whereIn("g.Avb_Id", cachedBags).query()
        };
    }

    public object LoadHisDutys(HisAuthInput input)
    {
        global::System.Func<List<string>, global::System.Data.DataTable> loadData = visitors =>
            DBCash.useSQL(MooSqlSetup.DefaultPosition)
                .select("w.Awi_Name,w.Awi_Code,w.Awi_Type,d.Adi_Name,d.Adi_Note")
                .from("AuthWordItem w full join AuthDutyItem d on w.Awi_WordBagId=d.Adi_WordBagId")
                .whereIn("d.Adi_VisitorBagId", visitors)
                .query();

        var now = DutyLoader.loadHisVisitorBag(input.Account, false);
        var cached = DutyLoader.loadHisVisitorBag(input.Account, true);
        return new { now = loadData(now), cached = loadData(cached) };
    }

    #endregion

    #region GoodsBag / GoodsItem

    public PagedResult<AuthGoodsBagDto> GetGoodsBagPage(AuthBagPageQuery query)
    {
        var list = _goodsBags.Repo.GetList(e => !e.IsDeleted).ToList();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            list = list.Where(e => MatchText(kw, e.Name, e.Code, e.Note)).ToList();
        }

        var page = PageHelper.Apply(list.OrderBy(e => e.Idx).Select(MapGoodsBag).ToList(),
            new PageQuery { Page = query.Page, PageSize = query.PageSize });
        if (page.Records.Count == 0) return page;

        var ids = page.Records.Select(x => x.Id).ToList();
        var counts = _goodsItems.Repo.GetList(g => ids.Contains(g.GoodsBagId) && g.Type == "1")
            .GroupBy(g => g.GoodsBagId)
            .ToDictionary(g => g.Key, g => g.Count());
        foreach (var item in page.Records)
            item.MenuBindCount = counts.GetValueOrDefault(item.Id);
        return page;
    }

    public AuthGoodsBagDto? GetGoodsBag(Guid id)
    {
        var row = _goodsBags.Repo.GetById(id);
        return row is null || row.IsDeleted ? null : MapGoodsBag(row);
    }

    public Guid SaveGoodsBag(AuthGoodsBagDto input)
    {
        if (input.Id == Guid.Empty)
        {
            var entity = new AuthGoodsBag
            {
                Id = Guid.CreateVersion7(),
                Name = input.Name,
                Code = input.Code,
                Note = input.Note,
                Remark = input.Remark,
                Type = input.Type,
                Idx = input.Idx,
                TenantId = _tenant.TenantId,
                CreateTime = DateTime.Now
            };
            _goodsBags.Repo.Insert(entity);
            return entity.Id;
        }

        var existing = _goodsBags.Repo.GetById(input.Id)
                     ?? throw new InvalidOperationException("资源集合不存在");
        existing.Name = input.Name;
        existing.Code = input.Code;
        existing.Note = input.Note;
        existing.Remark = input.Remark;
        existing.Type = input.Type;
        existing.Idx = input.Idx;
        existing.UpdateTime = DateTime.Now;
        _goodsBags.Repo.Update(existing);
        return existing.Id;
    }

    public void DeleteGoodsBag(Guid id)
    {
        var existing = _goodsBags.Repo.GetById(id)
                     ?? throw new InvalidOperationException("资源集合不存在");
        existing.IsDeleted = true;
        existing.UpdateTime = DateTime.Now;
        _goodsBags.Repo.Update(existing);

        var items = _goodsItems.Repo.GetList(g => g.GoodsBagId == id);
        foreach (var item in items)
            _goodsItems.Repo.DeleteById(item.Id);
    }

    public List<AuthGoodsItemDto> LoadBindKeys(Guid goodsBagId, string type)
    {
        return _goodsItems.Repo.GetList(g =>
                g.GoodsBagId == goodsBagId && g.Type == type)
            .Select(MapGoodsItem)
            .ToList();
    }

    public void SaveBindKeys(MenuBindInput input)
    {
        if (input.Type != "1")
            throw new InvalidOperationException("首期仅支持菜单绑定(Gi_Type=1)");

        var existing = _goodsItems.Repo.GetList(g => g.GoodsBagId == input.GoodsBagId && g.Type == input.Type);
        var hasOids = existing.Select(e => e.Oid).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var menus = _menus.Repo.GetList(m => input.CheckMenus.Contains(m.Id.ToString()));
        var menuMap = menus.ToDictionary(m => m.Id.ToString(), StringComparer.OrdinalIgnoreCase);

        foreach (var menuId in input.CheckMenus.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            if (hasOids.Contains(menuId)) continue;
            menuMap.TryGetValue(menuId, out var menu);
            _goodsItems.Repo.Insert(new AuthGoodsItem
            {
                Id = Guid.CreateVersion7(),
                GoodsBagId = input.GoodsBagId,
                Type = input.Type,
                Oid = menuId,
                Name = menu?.Title,
                Code = menu?.Path ?? menu?.Permission,
                CreateTime = DateTime.Now,
                TenantId = _tenant.TenantId
            });
        }

        foreach (var item in existing)
        {
            if (!input.CheckMenus.Contains(item.Oid, StringComparer.OrdinalIgnoreCase))
                _goodsItems.Repo.DeleteById(item.Id);
        }
    }

    #endregion

    #region Visitor

    public PagedResult<AuthVisitorBagDto> GetVisitorBagPage(AuthBagPageQuery query)
    {
        var list = _visitorBags.Repo.GetList(e => !e.IsDeleted).ToList();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            list = list.Where(e => MatchText(kw, e.Name, e.Code, e.Note, e.KeyWord)).ToList();
        }

        return PageHelper.Apply(list.OrderBy(e => e.Idx).Select(MapVisitorBag).ToList(),
            new PageQuery { Page = query.Page, PageSize = query.PageSize });
    }

    public AuthVisitorBagDto? GetVisitorBag(Guid id)
    {
        var row = _visitorBags.Repo.GetById(id);
        return row is null || row.IsDeleted ? null : MapVisitorBag(row);
    }

    public Guid SaveVisitorBag(AuthVisitorBagDto input)
    {
        if (input.Id == Guid.Empty)
        {
            var entity = new AuthVisitorBag
            {
                Id = Guid.CreateVersion7(),
                Name = input.Name,
                Code = input.Code,
                Note = input.Note,
                Remark = input.Remark,
                Type = input.Type,
                KeyWord = input.KeyWord,
                Idx = input.Idx,
                ParentId = input.ParentId,
                TenantId = _tenant.TenantId,
                CreateTime = DateTime.Now
            };
            _visitorBags.Repo.Insert(entity);
            return entity.Id;
        }

        var existing = _visitorBags.Repo.GetById(input.Id)
                     ?? throw new InvalidOperationException("访客集合不存在");
        existing.Name = input.Name;
        existing.Code = input.Code;
        existing.Note = input.Note;
        existing.Remark = input.Remark;
        existing.Type = input.Type;
        existing.KeyWord = input.KeyWord;
        existing.Idx = input.Idx;
        existing.ParentId = input.ParentId;
        existing.UpdateTime = DateTime.Now;
        _visitorBags.Repo.Update(existing);
        return existing.Id;
    }

    public void DeleteVisitorBag(Guid id)
    {
        var existing = _visitorBags.Repo.GetById(id)
                     ?? throw new InvalidOperationException("访客集合不存在");
        existing.IsDeleted = true;
        existing.UpdateTime = DateTime.Now;
        _visitorBags.Repo.Update(existing);

        var items = _visitorItems.Repo.GetList(v => v.VisitorBagId == id);
        foreach (var item in items)
            _visitorItems.Repo.DeleteById(item.Id!.Value);
    }

    public PagedResult<AuthVisitorItemDto> GetVisitorItemPage(AuthBagPageQuery query)
    {
        var list = _visitorItems.Repo.GetList(v =>
            (!query.BagId.HasValue || v.VisitorBagId == query.BagId) &&
            (string.IsNullOrWhiteSpace(query.Type) || v.Type == query.Type));
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            list = list.Where(v =>
                (v.Name?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (v.Code?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }

        return PageHelper.Apply(list.OrderBy(v => v.Idx).Select(MapVisitorItem).ToList(),
            new PageQuery { Page = query.Page, PageSize = query.PageSize });
    }

    public int BatchSaveVisitorItems(VisitorItemBatchInput input)
    {
        if (input.VisitorBagId == Guid.Empty)
            throw new ArgumentException("访客集合不能为空");
        if (_visitorBags.Repo.GetById(input.VisitorBagId) is null)
            throw new InvalidOperationException("访客集合不存在");

        var existing = _visitorItems.Repo.GetList(v => v.VisitorBagId == input.VisitorBagId);
        var added = 0;
        foreach (var item in input.Items)
        {
            var type = item.Type ?? "1";
            var dup = existing.Any(e =>
                string.Equals(e.Type, type, StringComparison.OrdinalIgnoreCase) &&
                ((!string.IsNullOrWhiteSpace(item.Oid) &&
                  string.Equals(e.Oid, item.Oid, StringComparison.OrdinalIgnoreCase)) ||
                 (!string.IsNullOrWhiteSpace(item.Code) &&
                  string.Equals(e.Code, item.Code, StringComparison.OrdinalIgnoreCase))));
            if (dup) continue;

            var entity = new AuthVisitorItem
            {
                Id = Guid.CreateVersion7(),
                VisitorBagId = input.VisitorBagId,
                Type = type,
                Name = item.Name,
                Code = item.Code,
                Oid = item.Oid,
                IsOn = item.IsOn,
                Idx = item.Idx,
                TenantId = _tenant.TenantId
            };
            _visitorItems.Repo.Insert(entity);
            existing.Add(entity);
            added++;
        }

        return added;
    }

    public Guid? SaveVisitorItem(AuthVisitorItemDto input)
    {
        if (input.Id == Guid.Empty)
        {
            var entity = new AuthVisitorItem
            {
                Id = Guid.CreateVersion7(),
                VisitorBagId = input.VisitorBagId,
                Type = input.Type,
                Name = input.Name,
                Code = input.Code,
                Oid = input.Oid,
                IsOn = input.IsOn,
                Idx = input.Idx,
                TenantId = _tenant.TenantId
            };
            _visitorItems.Repo.Insert(entity);
            return entity.Id;
        }

        var existing = _visitorItems.Repo.GetById(input.Id)
                     ?? throw new InvalidOperationException("访客项不存在");
        existing.VisitorBagId = input.VisitorBagId;
        existing.Type = input.Type;
        existing.Name = input.Name;
        existing.Code = input.Code;
        existing.Oid = input.Oid;
        existing.IsOn = input.IsOn;
        existing.Idx = input.Idx;
        _visitorItems.Repo.Update(existing);
        return existing.Id;
    }

    public void DeleteVisitorItem(Guid id) => _visitorItems.Repo.DeleteById(id);

    #endregion

    #region Word

    public PagedResult<AuthWordBagDto> GetWordBagPage(AuthBagPageQuery query)
    {
        var list = _wordBags.Repo.GetList(e => !e.IsDeleted).ToList();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            list = list.Where(e => MatchText(kw, e.Name, e.Code, e.Note)).ToList();
        }

        return PageHelper.Apply(list.OrderBy(e => e.Idx).Select(MapWordBag).ToList(),
            new PageQuery { Page = query.Page, PageSize = query.PageSize });
    }

    public AuthWordBagDto? GetWordBag(Guid id)
    {
        var row = _wordBags.Repo.GetById(id);
        return row is null || row.IsDeleted ? null : MapWordBag(row);
    }

    public Guid SaveWordBag(AuthWordBagDto input)
    {
        if (input.Id == Guid.Empty)
        {
            var entity = new AuthWordBag
            {
                Id = Guid.CreateVersion7(),
                Name = input.Name,
                Code = input.Code,
                Note = input.Note,
                Remark = input.Remark,
                Type = input.Type,
                Idx = input.Idx,
                TenantId = _tenant.TenantId,
                CreateTime = DateTime.Now
            };
            _wordBags.Repo.Insert(entity);
            return entity.Id;
        }

        var existing = _wordBags.Repo.GetById(input.Id)
                     ?? throw new InvalidOperationException("词条集合不存在");
        existing.Name = input.Name;
        existing.Code = input.Code;
        existing.Note = input.Note;
        existing.Remark = input.Remark;
        existing.Type = input.Type;
        existing.Idx = input.Idx;
        existing.UpdateTime = DateTime.Now;
        _wordBags.Repo.Update(existing);
        return existing.Id;
    }

    public void DeleteWordBag(Guid id)
    {
        var existing = _wordBags.Repo.GetById(id)
                     ?? throw new InvalidOperationException("词条集合不存在");
        existing.IsDeleted = true;
        existing.UpdateTime = DateTime.Now;
        _wordBags.Repo.Update(existing);

        var items = _wordItems.Repo.GetList(v => v.WordBagId == id);
        foreach (var item in items)
            _wordItems.Repo.DeleteById(item.Id);
    }

    public PagedResult<AuthWordItemDto> GetWordItemPage(AuthBagPageQuery query)
    {
        var list = _wordItems.Repo.GetList(v => !query.BagId.HasValue || v.WordBagId == query.BagId.Value);
        return PageHelper.Apply(list.OrderBy(v => v.Idx).Select(MapWordItem).ToList(),
            new PageQuery { Page = query.Page, PageSize = query.PageSize });
    }

    public Guid SaveWordItem(AuthWordItemDto input)
    {
        if (input.Id == Guid.Empty)
        {
            var entity = new AuthWordItem
            {
                Id = Guid.CreateVersion7(),
                WordBagId = input.WordBagId,
                WordDefineId = input.WordDefineId,
                Name = input.Name,
                Code = input.Code,
                Type = input.Type,
                Idx = input.Idx,
                TenantId = _tenant.TenantId
            };
            _wordItems.Repo.Insert(entity);
            return entity.Id;
        }

        var existing = _wordItems.Repo.GetById(input.Id)
                     ?? throw new InvalidOperationException("词条项不存在");
        existing.WordBagId = input.WordBagId;
        existing.WordDefineId = input.WordDefineId;
        existing.Name = input.Name;
        existing.Code = input.Code;
        existing.Type = input.Type;
        existing.Idx = input.Idx;
        _wordItems.Repo.Update(existing);
        return existing.Id;
    }

    public void DeleteWordItem(Guid id) => _wordItems.Repo.DeleteById(id);

    public PagedResult<AuthWordDefineDto> GetWordDefinePage(AuthBagPageQuery query)
    {
        var list = _wordDefines.Repo.GetList(e => !e.IsDeleted).ToList();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            list = list.Where(e => MatchText(kw, e.Name, e.Code, e.Note, e.Type)).ToList();
        }

        return PageHelper.Apply(list.OrderBy(e => e.Idx).Select(MapWordDefine).ToList(),
            new PageQuery { Page = query.Page, PageSize = query.PageSize });
    }

    public AuthWordDefineDto? GetWordDefine(Guid id)
    {
        var row = _wordDefines.Repo.GetById(id);
        return row is null || row.IsDeleted ? null : MapWordDefine(row);
    }

    public Guid SaveWordDefine(AuthWordDefineDto input)
    {
        if (input.Id == Guid.Empty)
        {
            var entity = new AuthWordDefine
            {
                Id = Guid.CreateVersion7(),
                Name = input.Name,
                Code = input.Code,
                Type = input.Type,
                Para = input.Para,
                Parser = input.Parser,
                Note = input.Note,
                Idx = input.Idx,
                IsOn = input.IsOn,
                TenantId = _tenant.TenantId,
                CreateTime = DateTime.Now
            };
            _wordDefines.Repo.Insert(entity);
            return entity.Id;
        }

        var existing = _wordDefines.Repo.GetById(input.Id)
                     ?? throw new InvalidOperationException("词条定义不存在");
        existing.Name = input.Name;
        existing.Code = input.Code;
        existing.Type = input.Type;
        existing.Para = input.Para;
        existing.Parser = input.Parser;
        existing.Note = input.Note;
        existing.Idx = input.Idx;
        existing.IsOn = input.IsOn;
        existing.UpdateTime = DateTime.Now;
        _wordDefines.Repo.Update(existing);
        return existing.Id;
    }

    public void DeleteWordDefine(Guid id)
    {
        var existing = _wordDefines.Repo.GetById(id)
                     ?? throw new InvalidOperationException("词条定义不存在");
        existing.IsDeleted = true;
        existing.UpdateTime = DateTime.Now;
        _wordDefines.Repo.Update(existing);
    }

    public List<WordSelectItem> LoadWordsForSelect()
    {
        return _wordDefines.Repo.GetList(d => d.IsOn && !d.IsDeleted)
            .OrderBy(d => d.Idx)
            .Select(d => new WordSelectItem
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                Type = d.Type
            }).ToList();
    }

    public List<BagSelectItem> LoadVisitorBagsForSelect() =>
        _visitorBags.Repo.GetList(e => !e.IsDeleted)
            .OrderBy(e => e.Idx)
            .Select(e => new BagSelectItem { Id = e.Id, Name = e.Name, Code = e.Code })
            .ToList();

    public List<BagSelectItem> LoadGoodsBagsForSelect() =>
        _goodsBags.Repo.GetList(e => !e.IsDeleted)
            .OrderBy(e => e.Idx)
            .Select(e => new BagSelectItem { Id = e.Id, Name = e.Name, Code = e.Code })
            .ToList();

    public List<BagSelectItem> LoadWordBagsForSelect() =>
        _wordBags.Repo.GetList(e => !e.IsDeleted)
            .OrderBy(e => e.Idx)
            .Select(e => new BagSelectItem { Id = e.Id, Name = e.Name, Code = e.Code })
            .ToList();

    #endregion

    private static PagedResult<TDto> PageRepo<TEntity, TDto>(
        MooRepository<TEntity> repo,
        AuthBagPageQuery query,
        Func<TEntity, bool> filter,
        Func<TEntity, TDto> map) where TEntity : class, new()
    {
        var list = repo.Repo.GetList().Where(filter).ToList();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var kw = query.Keyword.Trim();
            list = list.Where(e => e.ToString()?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
        }

        return PageHelper.Apply(list.Select(map).ToList(),
            new PageQuery { Page = query.Page, PageSize = query.PageSize });
    }

    private static bool MatchText(string kw, params string?[] fields) =>
        fields.Any(f => f?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false);

    private static AuthDutyItemDto MapDuty(AuthDutyItem e) => new()
    {
        Id = e.Id, Name = e.Name, Code = e.Code, Note = e.Note, Remark = e.Remark, Idx = e.Idx,
        VisitorBagId = e.VisitorBagId, WordBagId = e.WordBagId, GoodsBagId = e.GoodsBagId,
        CreateTime = e.CreateTime, UpdateTime = e.UpdateTime
    };

    private static AuthDutyItemDto MapDuty(global::System.Data.DataRow row) => new()
    {
        Id = Guid.Parse(row["Adi_Id"].ToString()!),
        Name = row.getString("Adi_Name", ""),
        Code = row.getString("Adi_Code", ""),
        Note = row.getString("Adi_Note", ""),
        Remark = row.getString("Adi_Remark", ""),
        Idx = row.getInt("Adi_Idx", 0),
        VisitorBagId = Guid.TryParse(row.getString("Adi_VisitorBagId", ""), out var vb) ? vb : null,
        WordBagId = Guid.TryParse(row.getString("Adi_WordBagId", ""), out var wb) ? wb : null,
        GoodsBagId = Guid.TryParse(row.getString("Adi_GoodsBagId", ""), out var gb) ? gb : null,
        VisitorBagName = row.Table.Columns.Contains("VisitorBagName") ? row.getString("VisitorBagName", "") : null,
        WordBagName = row.Table.Columns.Contains("WordBagName") ? row.getString("WordBagName", "") : null,
        GoodsBagName = row.Table.Columns.Contains("GoodsBagName") ? row.getString("GoodsBagName", "") : null,
        CreateTime = row.getDateTime("Adi_CreateTime", DateTime.Now),
        UpdateTime = row.getDateTimeOrNull("Adi_UpdateTime")
    };

    private static AuthGoodsBagDto MapGoodsBag(AuthGoodsBag e) => new()
    {
        Id = e.Id, Name = e.Name, Code = e.Code, Note = e.Note, Remark = e.Remark, Type = e.Type, Idx = e.Idx
    };

    private static AuthGoodsItemDto MapGoodsItem(AuthGoodsItem e) => new()
    {
        Id = e.Id, GoodsBagId = e.GoodsBagId, Type = e.Type, Oid = e.Oid, Name = e.Name, Code = e.Code
    };

    private static AuthVisitorBagDto MapVisitorBag(AuthVisitorBag e) => new()
    {
        Id = e.Id, Name = e.Name, Code = e.Code, Note = e.Note, Remark = e.Remark, Type = e.Type,
        KeyWord = e.KeyWord, Idx = e.Idx, ParentId = e.ParentId
    };

    private static AuthVisitorItemDto MapVisitorItem(AuthVisitorItem e) => new()
    {
        Id = e.Id, VisitorBagId = e.VisitorBagId, Type = e.Type, Name = e.Name, Code = e.Code,
        Oid = e.Oid, IsOn = e.IsOn, Idx = e.Idx
    };

    private static AuthWordBagDto MapWordBag(AuthWordBag e) => new()
    {
        Id = e.Id, Name = e.Name, Code = e.Code, Note = e.Note, Remark = e.Remark, Type = e.Type, Idx = e.Idx
    };

    private static AuthWordItemDto MapWordItem(AuthWordItem e) => new()
    {
        Id = e.Id, WordBagId = e.WordBagId, WordDefineId = e.WordDefineId, Name = e.Name,
        Code = e.Code, Type = e.Type, Idx = e.Idx
    };

    private static AuthWordDefineDto MapWordDefine(AuthWordDefine e) => new()
    {
        Id = e.Id, Name = e.Name, Code = e.Code, Type = e.Type, Para = e.Para, Parser = e.Parser,
        Note = e.Note, Idx = e.Idx, IsOn = e.IsOn
    };
}
