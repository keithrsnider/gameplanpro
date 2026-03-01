using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters;

public class HttpStatusCodeFilter : IResultFilter
{
	public void OnResultExecuting(ResultExecutingContext context)
	{
		var method = context.HttpContext.Request.Method;

		if (method == HttpMethods.Delete && context.Result is EmptyResult)
		{
			context.Result = new StatusCodeResult(StatusCodes.Status204NoContent);
		}
		else if (method == HttpMethods.Post && context.Result is ObjectResult objectResult)
		{
			objectResult.StatusCode = StatusCodes.Status201Created;
		}
	}

	public void OnResultExecuted(ResultExecutedContext context) { }
}
