using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.AspNet;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.Models;
using SleveenAdmin.System.AuthBag.Dtos;
using SleveenAdmin.System.AuthBag.Service;

namespace SleveenAdmin.System.AuthBag.Controller;

[Authorize]
[ApiController]
[Route("api/authBag")]
public class AuthBagController : ControllerBase
{
    private readonly AuthBagService _service;

    public AuthBagController(AuthBagService service) => _service = service;

    private IAuthUserContext RequireUser() =>
        AuthUserContext.FromClaims(User) ?? throw new UnauthorizedAccessException("未登录");

    [HttpGet("dutyItem/page")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult DutyPage([FromQuery] AuthBagPageQuery query) => Ok(_service.GetDutyPage(query));

    [HttpGet("dutyItem/{id:guid}")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult DutyGet(Guid id)
    {
        var dto = _service.GetDuty(id);
        return dto is null ? NotFound(ApiResult.Fail("权限条目不存在")) : Ok(dto);
    }

    [HttpPost("dutyItem")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult DutySave([FromBody] AuthDutyItemDto input) =>
        Ok(new { id = _service.SaveDuty(input, RequireUser()) });

    [HttpDelete("dutyItem/{id:guid}")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult DutyDelete(Guid id)
    {
        _service.DeleteDuty(id);
        return Ok();
    }

    [HttpPost("dutyItem/cleanCache")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult CleanCache([FromBody] CleanCacheInput input) =>
        Ok(new { count = _service.CleanCache(input) });

    [HttpPost("dutyItem/loadHisVisitors")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult LoadHisVisitors([FromBody] HisAuthInput input) => Ok(_service.LoadHisVisitors(input));

    [HttpPost("dutyItem/loadHisDutys")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult LoadHisDutys([FromBody] HisAuthInput input) => Ok(_service.LoadHisDutys(input));

    [HttpGet("goodsBag/page")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult GoodsBagPage([FromQuery] AuthBagPageQuery query) => Ok(_service.GetGoodsBagPage(query));

    [HttpGet("goodsBag/{id:guid}")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult GoodsBagGet(Guid id)
    {
        var dto = _service.GetGoodsBag(id);
        return dto is null ? NotFound(ApiResult.Fail("资源集合不存在")) : Ok(dto);
    }

    [HttpPost("goodsBag")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult GoodsBagSave([FromBody] AuthGoodsBagDto input) =>
        Ok(new { id = _service.SaveGoodsBag(input) });

    [HttpDelete("goodsBag/{id:guid}")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult GoodsBagDelete(Guid id)
    {
        _service.DeleteGoodsBag(id);
        return Ok();
    }

    [HttpGet("goodsItem/bindKeys")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult LoadBindKeys([FromQuery] Guid goodsBagId, [FromQuery] string type = "1") =>
        Ok(_service.LoadBindKeys(goodsBagId, type));

    [HttpPost("goodsItem/bindKeys")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult SaveBindKeys([FromBody] MenuBindInput input)
    {
        _service.SaveBindKeys(input);
        return Ok();
    }

    [HttpGet("visitorBag/page")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult VisitorBagPage([FromQuery] AuthBagPageQuery query) => Ok(_service.GetVisitorBagPage(query));

    [HttpGet("visitorBag/{id:guid}")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult VisitorBagGet(Guid id)
    {
        var dto = _service.GetVisitorBag(id);
        return dto is null ? NotFound(ApiResult.Fail("访客集合不存在")) : Ok(dto);
    }

    [HttpPost("visitorBag")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult VisitorBagSave([FromBody] AuthVisitorBagDto input) =>
        Ok(new { id = _service.SaveVisitorBag(input) });

    [HttpDelete("visitorBag/{id:guid}")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult VisitorBagDelete(Guid id)
    {
        _service.DeleteVisitorBag(id);
        return Ok();
    }

    [HttpGet("visitorItem/page")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult VisitorItemPage([FromQuery] AuthBagPageQuery query) => Ok(_service.GetVisitorItemPage(query));

    [HttpPost("visitorItem")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult VisitorItemSave([FromBody] AuthVisitorItemDto input) =>
        Ok(new { id = _service.SaveVisitorItem(input) });

    [HttpPost("visitorItem/batch")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult VisitorItemBatch([FromBody] VisitorItemBatchInput input) =>
        Ok(new { count = _service.BatchSaveVisitorItems(input) });

    [HttpDelete("visitorItem/{id:guid}")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult VisitorItemDelete(Guid id)
    {
        _service.DeleteVisitorItem(id);
        return Ok();
    }

    [HttpGet("visitorBag/select")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult VisitorBagSelect() => Ok(_service.LoadVisitorBagsForSelect());

    [HttpGet("goodsBag/select")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult GoodsBagSelect() => Ok(_service.LoadGoodsBagsForSelect());

    [HttpGet("wordBag/select")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult WordBagSelect() => Ok(_service.LoadWordBagsForSelect());

    [HttpGet("wordBag/page")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult WordBagPage([FromQuery] AuthBagPageQuery query) => Ok(_service.GetWordBagPage(query));

    [HttpGet("wordBag/{id:guid}")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult WordBagGet(Guid id)
    {
        var dto = _service.GetWordBag(id);
        return dto is null ? NotFound(ApiResult.Fail("词条集合不存在")) : Ok(dto);
    }

    [HttpPost("wordBag")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult WordBagSave([FromBody] AuthWordBagDto input) =>
        Ok(new { id = _service.SaveWordBag(input) });

    [HttpDelete("wordBag/{id:guid}")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult WordBagDelete(Guid id)
    {
        _service.DeleteWordBag(id);
        return Ok();
    }

    [HttpGet("wordItem/page")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult WordItemPage([FromQuery] AuthBagPageQuery query) => Ok(_service.GetWordItemPage(query));

    [HttpPost("wordItem")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult WordItemSave([FromBody] AuthWordItemDto input) =>
        Ok(new { id = _service.SaveWordItem(input) });

    [HttpDelete("wordItem/{id:guid}")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult WordItemDelete(Guid id)
    {
        _service.DeleteWordItem(id);
        return Ok();
    }

    [HttpGet("wordDefine/page")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult WordDefinePage([FromQuery] AuthBagPageQuery query) => Ok(_service.GetWordDefinePage(query));

    [HttpGet("wordDefine/{id:guid}")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult WordDefineGet(Guid id)
    {
        var dto = _service.GetWordDefine(id);
        return dto is null ? NotFound(ApiResult.Fail("词条定义不存在")) : Ok(dto);
    }

    [HttpPost("wordDefine")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult WordDefineSave([FromBody] AuthWordDefineDto input) =>
        Ok(new { id = _service.SaveWordDefine(input) });

    [HttpDelete("wordDefine/{id:guid}")]
    [Permission("authBag:edit")]
    [ApiResult]
    public IActionResult WordDefineDelete(Guid id)
    {
        _service.DeleteWordDefine(id);
        return Ok();
    }

    [HttpGet("wordDefine/select")]
    [Permission("authBag:view")]
    [ApiResult]
    public IActionResult WordDefineSelect() => Ok(_service.LoadWordsForSelect());
}
