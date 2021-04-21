using Hl7.Fhir.OpenAPI.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Hl7.Fhir.OpenAPI.Extensions
{
    public static class ServiceExtensions
    {
        // Add API Versioning
        // The default version is 1.0
        // And we're going to read the version number from the media type
        // Incoming requests should have a accept header like this: Accept: application/json;v=1.0
        public static void AddApiVersioningExtension(this IServiceCollection services)
        {
            services.AddApiVersioning(config =>
            {
                // Default API Version
                config.DefaultApiVersion = new ApiVersion(1, 0);
                // use default version when version is not specified
                config.AssumeDefaultVersionWhenUnspecified = true;
                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
            });
        }

        // More info: https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-3.1
        public static void AddCorsPolicy(this IServiceCollection services, string policyName)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName,
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("X-Pagination"));
            });
        }

        public static void AddSwaggerExtension(this IServiceCollection services, string apiName)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = apiName,
                    Version = "v1",
                    Description = "OpenAPI demo for custom EHR integration with FHIR server.<br>**NOTE: NOT ALL RESOURCES HAVE BEEN IMPLEMENTED!",
                    Contact = new OpenApiContact
                    {
                        Name = "Email",
                        Email = "matjaz.bravc@gmail.com",
                        Url = new Uri("https://matjazbravc.github.io/")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Licenced under MIT license",
                        Url = new Uri("http://opensource.org/licenses/mit-license.php")
                    },
                     // Adding a Logo to ReDoc page
                    Extensions = new Dictionary<string, IOpenApiExtension>
                    {
                        {
                            "x-logo", new OpenApiObject
                            {
                                { "url", new OpenApiString("/Resources/Images/FhirLogo.png") },
                                { "altText", new OpenApiString("The Logo") }
                            }
                        }
                    }
                });
                options.OperationFilter<SwaggerFileUploadOperationFilter>();
                var xmlDocFile = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                if (File.Exists(xmlDocFile))
                {
                    options.IncludeXmlComments(xmlDocFile);
                }
                options.DescribeAllParametersInCamelCase();
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });
        }
    }
}