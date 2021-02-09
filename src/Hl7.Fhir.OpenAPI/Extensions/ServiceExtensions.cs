using Hl7.Fhir.OpenAPI.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Linq;
using System.Reflection;
using System;
using Hl7.Fhir.OpenAPI.Filters;
using System.Collections.Generic;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Any;

namespace Hl7.Fhir.OpenAPI.Extensions
{
    public static class ServiceExtensions
    {
        // More info: https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-3.1
        public static void AddCorsPolicy(this IServiceCollection serviceCollection, string corsPolicyName)
        {
            serviceCollection.AddCors(options =>
            {
                options.AddPolicy(corsPolicyName,
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
        }

        public static void ConfigureSwagger(this IServiceCollection services, string apiName, bool includeXmlDocumentation = true)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1.0", new OpenApiInfo
                {
                    Title = apiName,
                    Version = "v1.0",
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
                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var actionApiVersionModel = apiDesc.ActionDescriptor.GetApiVersionModel();
                    // Would mean this action is unversioned and should be included everywhere
                    if (actionApiVersionModel == null)
                    {
                        return true;
                    }
                    return actionApiVersionModel.DeclaredApiVersions.Any() ? actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v}" == docName) : actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v.ToString()}" == docName);
                });
                if (includeXmlDocumentation)
                {
                    var xmlDocFile = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                    if (File.Exists(xmlDocFile))
                    {
                        options.IncludeXmlComments(xmlDocFile);
                    }
                }
                options.DescribeAllParametersInCamelCase();
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });
        }

        public static IApplicationBuilder UseApiLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiLogging>();
        }

        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandling>();
        }
    }
}