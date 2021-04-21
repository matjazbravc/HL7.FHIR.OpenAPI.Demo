using Hl7.Fhir.OpenAPI.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Hl7.Fhir.OpenAPI.Extensions
{
    public static class AppExtensions
    {
        // Swagger Marker - Do Not Delete
        public static void UseSwaggerExtension(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });
        }

        public static IApplicationBuilder UseApiLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiLogging>();
        }

        public static void UseErrorHandlingMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
