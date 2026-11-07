using Microsoft.AspNetCore.Mvc;
using SleveenAdmin.Foundation.Biz;
using SleveenAdmin.Officeman.Ebook.Service;

namespace SleveenAdmin.Officeman.Ebook.Controller;

[Route("biz/saebhome")]
public class BizSaEbHomeController : BizControllerBase
{
    private readonly BizSaEbHomeService _service;

    public BizSaEbHomeController(BizSaEbHomeService service) => _service = service;

    protected override BizServiceBase Service => _service;

    public override string BizName => "Biz_Sa_EbHome";

    [HttpPost("ebook/ebhome/dashboard")]
    public IActionResult Dashboard([FromBody] DashboardInput? input)
        => Ok(_service.GetDashboard(input?.Days ?? 7));

    public sealed class DashboardInput
    {
        public int Days { get; set; } = 7;
    }
}
