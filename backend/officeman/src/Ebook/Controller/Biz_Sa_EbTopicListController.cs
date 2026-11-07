using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.Officeman.Ebook.Service;

namespace SleveenAdmin.Officeman.Ebook.Controller;

[Route("biz/saebtopiclist")]
public class BizSaEbTopicListController : BizControllerBase
{
    private readonly BizSaEbTopicService _service;

    public BizSaEbTopicListController(BizSaEbTopicService service) => _service = service;

    protected override BizServiceBase Service => _service;

    public override string BizName => "Biz_Sa_EbTopicList";

    [HttpPost("ebook/ebtopic/qlist")]
    public IActionResult QList([FromBody] BizQueryInput input) => QListCore(input);

    [HttpGet("ebook/ebtopic/qone/{id:guid}")]
    public IActionResult QOne(Guid id) => QOneCore(id);

    [HttpPost("ebook/ebtopic/save")]
    public IActionResult Save([FromBody] JsonElement rows) => SaveCore(rows);

    [HttpPost("ebook/ebtopic/rmbyids")]
    public IActionResult Remove([FromBody] JsonElement para) => RemoveCore(para);

    [HttpPost("ebook/ebtopic/stats")]
    public IActionResult Stats() => Ok(_service.GetStats());

    [HttpPost("ebook/ebtopic/advance")]
    public IActionResult Advance([FromBody] JsonElement para)
    {
        var id = Guid.Empty;
        if (para.ValueKind == JsonValueKind.Object)
        {
            foreach (var key in new[] { "id", "topicId", "Sa_EbTopicOID" })
            {
                if (para.TryGetProperty(key, out var el) && Guid.TryParse(el.ToString(), out id))
                    break;
            }
        }

        return Ok(_service.AdvanceStage(id));
    }

    [HttpPost("ebook/ebtopic/listMat")]
    public IActionResult ListMat([FromBody] JsonElement para)
    {
        var id = Guid.Empty;
        if (para.ValueKind == JsonValueKind.Object)
        {
            foreach (var key in new[] { "id", "topicId", "Sa_EbTopicOID" })
            {
                if (para.TryGetProperty(key, out var el) && Guid.TryParse(el.ToString(), out id))
                    break;
            }
        }

        return Ok(_service.ListMaterials(id));
    }

    [HttpPost("ebook/ebtopic/bindMat")]
    public IActionResult BindMat([FromBody] JsonElement para) => Ok(_service.BindMaterial(para));

    [HttpPost("ebook/ebtopic/unbindMat")]
    public IActionResult UnbindMat([FromBody] JsonElement para) => Ok(_service.UnbindMaterial(para));
}
