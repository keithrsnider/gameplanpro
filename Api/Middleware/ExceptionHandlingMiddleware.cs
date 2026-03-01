using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
	private static readonly Dictionary<Type, Func<Exception, ProblemDetails>> ExceptionMap = new()
	{
		[typeof(Exceptions.NotFoundException)] = ex => CreateProblemDetails(
			HttpStatusCode.NotFound, "Not Found", ex.Message
		),
		[typeof(Exceptions.ForbiddenException)] = ex => CreateProblemDetails(
			HttpStatusCode.Forbidden, "Forbidden", ex.Message
		),
		[typeof(Exceptions.ConflictException)] = ex => CreateProblemDetails(
			HttpStatusCode.Conflict, "Conflict", ex.Message
		),
		[typeof(Exceptions.ValidationException)] = ex => CreateProblemDetails(
			HttpStatusCode.BadRequest, "Bad Request", ex.Message
		),
		[typeof(UnauthorizedAccessException)] = ex => CreateProblemDetails(
			HttpStatusCode.Unauthorized, "Unauthorized", ex.Message
		),
	};

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await next(context);
		}
		catch (Exception ex)
		{
			var problemDetails = ExceptionMap.TryGetValue(ex.GetType(), out var handler)
				? handler(ex)
				: CreateDefault(ex);

			if (problemDetails.Status >= 500)
				logger.LogError(ex, "Unhandled exception");
			else
				logger.LogWarning(ex, "Request exception: {Message}", ex.Message);

			context.Response.StatusCode = problemDetails.Status!.Value;
			context.Response.ContentType = "application/problem+json";
			await context.Response.WriteAsJsonAsync(problemDetails);
		}
	}

	private static ProblemDetails CreateProblemDetails(
		HttpStatusCode statusCode, string title, string detail)
	{
		return new ProblemDetails
		{
			Status = (int)statusCode,
			Title = title,
			Detail = detail,
		};
	}

	private static ProblemDetails CreateDefault(Exception ex)
	{
		return CreateProblemDetails(
			HttpStatusCode.InternalServerError,
			"Internal Server Error",
			"An unexpected error occurred."
		);
	}
}
