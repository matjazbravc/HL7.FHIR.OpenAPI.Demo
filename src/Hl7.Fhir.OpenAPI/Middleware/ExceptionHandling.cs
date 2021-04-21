using Hl7.Fhir.Common.Core.Errors.Base;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System;

namespace Hl7.Fhir.OpenAPI.Middleware
{
    /// <summary>
    /// Global Exception Handling Middleware
    /// </summary>
    public class ExceptionHandling : IMiddleware
    {
        private readonly ILogger _logger;

        public ExceptionHandling(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ApiLogging>();
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context).ConfigureAwait(false);
            }

            catch (Exception ex)
            {
                // Handle exception with modifying response with exception details
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Handle exception with modifying response
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="ex">Exception</param>
        /// <returns>Task</returns>
        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var errorMsg = ex.Message;
            if (ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message))
            {
                errorMsg = ex.InnerException.Message;
            }
            var exceptionType = ex.GetType();
            if (exceptionType == typeof(FhirOperationException))
            {
                var fhirEx = (FhirOperationException)ex;
                var errors = fhirEx.Outcome.Issue.Where(i => i.Severity == Model.OperationOutcome.IssueSeverity.Error);
                errorMsg = errors.FirstOrDefault()?.Diagnostics;
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            var apiError = new ApiError(context.Response.StatusCode, errorMsg, ex.GetType().ToString(), context.Request.Path);
            var result = JsonConvert.SerializeObject(apiError);

            _logger.LogError($"{apiError.Message}: {apiError.StatusDescription}, REQ: {apiError.RequestPath}");

            await context.Response.WriteAsync(result).ConfigureAwait(false);
        }
    }
}
