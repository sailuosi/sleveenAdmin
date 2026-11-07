using SleveenAdmin.Foundation.Ids;
using SleveenAdmin.Foundation.Models;

namespace SleveenAdmin.Tests;

public class SharedTests
{
    [Fact]
    public void GuidV7Generator_creates_version_7_guid()
    {
        var generator = new GuidV7Generator();
        var id = generator.Create();
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal('7', id.ToString()[14]);
    }

    [Fact]
    public void ApiResult_success_code_is_0000()
    {
        var result = ApiResult<string>.Ok("ok");
        Assert.Equal(ApiResultCodes.Success, result.Code);
        Assert.Equal("ok", result.Data);
    }
}
