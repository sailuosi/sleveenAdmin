using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mooSQL.data;
using sailflow.data;
using sailflow.engine.Client;
using sailflow.hub.Form;
using sailflow.WebUtility;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Hosting;
using SleveenAdmin.Sailflow.Service;
using SleveenAdmin.Sailflow.UnitScope;

namespace SleveenAdmin.Sailflow.Controller;

/// <summary>动态表单 CRUD（form-create conf/fields → fm_form）；Auth* / 软删落 FormEntity。</summary>
[Authorize]
[Route("api/[controller]/[action]/{id?}")]
[ApiController]
public class WfFormController : ControllerBase
{
    private readonly IFormService _formService;
    private readonly SailClient _sail;
    private readonly SailUnitAccess _access;

    public WfFormController(SailClient sail, SailUnitAccess access)
    {
        _sail = sail;
        _access = access;
        _formService = FormServiceFactory.Create(sail);
    }

    /// <summary>GET api/WfForm/GetList?unitId=</summary>
    [HttpGet]
    public ResponseResult<List<Form>> GetList([FromQuery] string? unitId = null)
    {
        try
        {
            var list = _formService.GetFormAll()
                .Where(f => SailSoftDelete.IsAlive(f.Deleted))
                .Where(f => _access.CanAccessResource(User, f.AuthType, f.AuthId, f.AuthCode, write: false))
                .Where(f => MatchUnitFilter(f, unitId))
                .ToList();
            return ResponseResult<List<Form>>.Success(list);
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
            if (form == null || SailSoftDelete.IsDeleted(form.Deleted))
                return ResponseResult<Form>.Error("form not found");
            _access.RequireAccess(User, form.AuthType, form.AuthId, form.AuthCode, write: false);
            return ResponseResult<Form>.Success(form);
        }
        catch (SailUnitAccessException ex)
        {
            return ResponseResult<Form>.Error(ex.Message);
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
            if (form == null) return ResponseResult.Error("body required");
            NormalizeFormAuth(form);

            var isNew = string.IsNullOrWhiteSpace(form.FormId);
            if (!isNew)
            {
                var existing = _formService.GetForm(form.FormId);
                if (existing == null || SailSoftDelete.IsDeleted(existing.Deleted))
                    return ResponseResult.Error("form not found");
                _access.RequireAccess(User, existing.AuthType, existing.AuthId, existing.AuthCode, write: true);
            }

            _access.RequireAccess(User, form.AuthType, form.AuthId, form.AuthCode, write: true);

            if (string.IsNullOrWhiteSpace(form.FormCode))
            {
                var orgNo = SailAuthScope.EqualsType(form.AuthType, SailAuthScope.Unit)
                    ? SailResourceCodes.ResolveOrgNo(form.AuthId)
                    : null;
                form.FormCode = SailResourceCodes.Build("form", form.AuthType, orgNo, form.FormName);
            }

            if (isNew)
            {
                form.Creator = CurrentUser.GetAccount(User) ?? CurrentUser.GetUserId(User)?.ToString() ?? "";
                form.CreatorName = User?.Identity?.Name ?? form.Creator;
                form.Deleted = 0;
            }

            var formId = _formService.SaveForm(form);
            var result = ResponseResult.Success("saved");
            result.NewId = formId;
            return result;
        }
        catch (SailUnitAccessException ex)
        {
            return ResponseResult.Error(ex.Message);
        }
        catch (Exception ex)
        {
            return ResponseResult.Error(ex.Message);
        }
    }

    /// <summary>DELETE/POST api/WfForm/Delete/{id}</summary>
    [HttpDelete]
    [HttpPost]
    public ResponseResult Delete(string id)
    {
        try
        {
            var form = _formService.GetForm(id);
            if (form == null || SailSoftDelete.IsDeleted(form.Deleted))
                return ResponseResult.Error("delete failed or not found");
            _access.RequireAccess(User, form.AuthType, form.AuthId, form.AuthCode, write: true);

            SailHostSchemaInitializer.EnsureHostTables(_sail);
            var clip = _sail.UseRunSQL().useClip();
            clip.from<SailProcessMetaEntity>(out var m);
            clip.where(() => m.FormId, id);
            var refs = clip.select(m).queryList()
                .Where(x => !string.IsNullOrWhiteSpace(x.FormId))
                .ToList();
            if (refs.Count > 0)
                return ResponseResult.Error($"表单仍被 {refs.Count} 个流程元数据引用，无法删除");

            var ok = _formService.DeleteForm(id);
            return ok ? ResponseResult.Success("deleted") : ResponseResult.Error("delete failed or not found");
        }
        catch (SailUnitAccessException ex)
        {
            return ResponseResult.Error(ex.Message);
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

    void NormalizeFormAuth(Form form)
    {
        if (string.IsNullOrWhiteSpace(form.AuthType))
        {
            if (!_access.IsPlatformAdmin(User))
                throw new ArgumentException("单位管理员须指定 UNIT 归属");
            return;
        }

        if (!SailAuthScope.IsKnown(form.AuthType))
            throw new ArgumentException("AuthType 须为 PUBLIC / UNIT / TEMPLATE");

        if (SailAuthScope.EqualsType(form.AuthType, SailAuthScope.Unit)
            && string.IsNullOrWhiteSpace(form.AuthId))
            throw new ArgumentException("UNIT 表单须填写 AuthId");

        if ((SailAuthScope.EqualsType(form.AuthType, SailAuthScope.Public)
             || SailAuthScope.EqualsType(form.AuthType, SailAuthScope.Template))
            && !_access.IsPlatformAdmin(User))
            throw new SailUnitAccessException("仅平台可维护公共/模板表单");
    }

    static bool MatchUnitFilter(Form f, string? unitId)
    {
        if (string.IsNullOrWhiteSpace(unitId)) return true;
        var uid = unitId.Trim();
        if (SailAuthScope.EqualsType(f.AuthType, SailAuthScope.Public)
            || SailAuthScope.EqualsType(f.AuthType, SailAuthScope.Template))
            return true;
        return SailAuthScope.EqualsType(f.AuthType, SailAuthScope.Unit)
               && string.Equals(f.AuthId, uid, StringComparison.OrdinalIgnoreCase);
    }
}
