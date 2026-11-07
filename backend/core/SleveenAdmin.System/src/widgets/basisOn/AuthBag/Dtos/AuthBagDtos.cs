namespace SleveenAdmin.System.AuthBag.Dtos;

public class AuthBagPageQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public Guid? BagId { get; set; }
    public string? Type { get; set; }
}

public class AuthDutyItemDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Note { get; set; }
    public string? Remark { get; set; }
    public int Idx { get; set; }
    public Guid? VisitorBagId { get; set; }
    public Guid? WordBagId { get; set; }
    public Guid? GoodsBagId { get; set; }
    public string? VisitorBagName { get; set; }
    public string? WordBagName { get; set; }
    public string? GoodsBagName { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}

public class AuthGoodsBagDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Note { get; set; }
    public string? Remark { get; set; }
    public string? Type { get; set; }
    public int Idx { get; set; }
    public int MenuBindCount { get; set; }
}

public class AuthGoodsItemDto
{
    public Guid Id { get; set; }
    public Guid GoodsBagId { get; set; }
    public string Type { get; set; } = "1";
    public string Oid { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Code { get; set; }
}

public class AuthVisitorBagDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Note { get; set; }
    public string? Remark { get; set; }
    public string? Type { get; set; }
    public string? KeyWord { get; set; }
    public int Idx { get; set; }
    public Guid? ParentId { get; set; }
}

public class AuthVisitorItemDto
{
    public Guid? Id { get; set; }
    public Guid? VisitorBagId { get; set; }
    public string? Type { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Oid { get; set; }
    public bool IsOn { get; set; } = true;
    public int Idx { get; set; }
}

public class AuthWordBagDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Note { get; set; }
    public string? Remark { get; set; }
    public string? Type { get; set; }
    public int Idx { get; set; }
}

public class AuthWordItemDto
{
    public Guid Id { get; set; }
    public Guid WordBagId { get; set; }
    public Guid? WordDefineId { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Type { get; set; }
    public int Idx { get; set; }
}

public class AuthWordDefineDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Type { get; set; }
    public string? Para { get; set; }
    public string? Parser { get; set; }
    public string? Note { get; set; }
    public int Idx { get; set; }
    public bool IsOn { get; set; } = true;
}

public class MenuBindInput
{
    public Guid GoodsBagId { get; set; }
    public string Type { get; set; } = "1";
    public List<string> CheckMenus { get; set; } = [];
}

public class HisAuthInput
{
    public string Account { get; set; } = string.Empty;
}

public class CleanCacheInput
{
    public string Mode { get; set; } = "1";
}

public class WordSelectItem
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Type { get; set; }
}

public class BagSelectItem
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
}

public class VisitorItemBatchInput
{
    public Guid VisitorBagId { get; set; }
    public List<AuthVisitorItemDto> Items { get; set; } = [];
}
