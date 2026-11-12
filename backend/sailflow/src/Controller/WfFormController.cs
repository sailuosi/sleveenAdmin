using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sailflow.engine.Client;
using sailflow.hub.Form;
using sailflow.WebUtility;

namespace SleveenAdmin.Saiflow.Controller;

/// <summary>动态表单 CRUD（form-create conf/fields → FbForm）。</summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfFormController : ControllerBase
{
    private readonly IFormService _formService;

    public WfFormController(SailClient sail)
    {
        _formService = FormServiceFactory.Create(sail);
    }

    /// <summary>GET api/WfForm/GetList</summary>
    [HttpGet]
    public ResponseResult<List<Form>> GetList()
    {
        try
        {
            return ResponseResult<List<Form>>.Success(_formService.GetFormAll().ToList());
        }
        catch (Exception ex)
        {
            return ResponseResult<List<Form>>.Error(ex.Message);
        }
    }

    /// <summary>GET api/WfForm/Get/{id}</summary>
    [HttpGet]
    public ResponseResult<Form> Get(string id)
    {
        try
        {
            var form = _formService.GetForm(id);
            return form == null
                ? ResponseResult<Form>.Error("form not found")
                : ResponseResult<Form>.Success(form);
        }
        catch (Exception ex)
        {
            return ResponseResult<Form>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfForm/Save</summary>
    [HttpPost]
    public ResponseResult Save([FromBody] Form form)
    {
        try
        {
            var formId = _formService.SaveForm(form);
            var result = ResponseResult.Success("saved");
            result.NewId = formId;
            return result;
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>DELETE api/WfForm/Delete/{id}</summary>
    [HttpDelete]
    [HttpPost]
    public ResponseResult Delete(string id)
    {
        try
        {
            var ok = _formService.DeleteForm(id);
            return ok ? ResponseResult.Success("deleted") : ResponseResult.Error("delete failed or not found");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfForm/GetFieldPermissions</summary>
    [HttpPost]
    public ResponseResult<List<FieldActivityEdit>> GetFieldPermissions([FromBody] FieldQuery query)
    {
        try
        {
            if (query == null || string.IsNullOrWhiteSpace(query.FormId))
                return ResponseResult<List<FieldActivityEdit>>.Error("FormId required");
            var list = _formService.GetFieldActivityEditByForm(query.FormId, query)?.ToList()
                       ?? new List<FieldActivityEdit>();
            return ResponseResult<List<FieldActivityEdit>>.Success(list);
        }
        catch (Exception ex)
        {
            return ResponseResult<List<FieldActivityEdit>>.Error(ex.Message);
        }
    }

    /// <summary>POST api/WfForm/SaveFieldPermissions</summary>
    [HttpPost]
    public ResponseResult SaveFieldPermissions([FromBody] FieldActivityEditInfo info)
    {
        try
        {
            _formService.SaveFieldActivityEditInfo(info);
            return ResponseResult.Success("saved");
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }
}
