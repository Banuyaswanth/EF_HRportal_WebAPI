using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EF_HRportal_WebAPI.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                ProblemDetails problemDetails = GetProblemDetails(ex);
                await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
            }
        }

        private ProblemDetails GetProblemDetails(Exception ex)
        {
            string traceId = Guid.NewGuid().ToString();
            return new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://httpstatuses.com/500",
                Title = "Something went wrong. Please try again after some time.",
                Detail = ex.Message,
                Instance = traceId
            };
        }
    }
}
