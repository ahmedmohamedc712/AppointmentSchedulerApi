using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace AppointmentScheduler.Exceptions;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch(BadRequestException ex)
        {
            await WriteProperProblemDetails(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch(NotFoundException ex)
        {
            await WriteProperProblemDetails(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch(ConflictException ex)
        {
            await WriteProperProblemDetails(context, StatusCodes.Status409Conflict, ex.Message);
        }
    }
    public async Task WriteProperProblemDetails(HttpContext context, int statusCode, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problem);
    }
}