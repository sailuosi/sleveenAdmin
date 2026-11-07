using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using SleveenAdmin.Foundation.DI;
using SleveenAdmin.Foundation.Models;

namespace SleveenAdmin.Foundation.AspNet;

public sealed class ApiResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not ObjectResult { Value: not ApiResult } objectResult)
            return;

        if (objectResult.Value is ApiResult)
            return;

        objectResult.Value = ApiResult<object>.Ok(objectResult.Value);
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}

public sealed class GlobalExceptionFilter : IExceptionFilter, IScoped
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) => _logger = logger;

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception");
        context.Result = new ObjectResult(ApiResult.Fail(context.Exception.Message))
        {
            StatusCode = 500
        };
        context.ExceptionHandled = true;
    }
}

public sealed class ApiResultAttribute : TypeFilterAttribute
{
    public ApiResultAttribute() : base(typeof(WrapApiResultFilter))
    {
    }
}

public sealed class WrapApiResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult { Value: ApiResult })
            return;

        if (context.Result is ObjectResult objectResult)
        {
            objectResult.Value = Wrap(objectResult.Value);
            return;
        }

        // ControllerBase.Ok() returns OkResult (no body). Frontend expects { code, msg, data }.
        if (context.Result is OkResult or EmptyResult)
        {
            context.Result = new ObjectResult(ApiResult.Ok()) { StatusCode = 200 };
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static object Wrap(object? value)
    {
        if (value is null)
            return ApiResult.Ok();

        var valueType = value.GetType();
        if (value is ApiResult)
            return value;

        if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(ApiResult<>))
            return value;

        var okMethod = typeof(ApiResult<>)
            .MakeGenericType(valueType)
            .GetMethod(nameof(ApiResult<object>.Ok), [valueType, typeof(string)]);
        return okMethod!.Invoke(null, [value, "success"])!;
    }
}
