using FluentValidation.AspNetCore;
using Hl7.Fhir.Common.Contracts.Converters;
using Hl7.Fhir.Common.Contracts.Dto;
using Hl7.Fhir.Common.Contracts.Models;
using Hl7.Fhir.Common.Core.Services;
using Hl7.Fhir.Model;
using Hl7.Fhir.OpenAPI.Converters;
using Hl7.Fhir.OpenAPI.Extensions;
using Hl7.Fhir.OpenAPI.Middleware;
using Hl7.Fhir.OpenAPI.Services.Options;
using Hl7.Fhir.OpenAPI.Services;
using Hl7.Fhir.OpenAPI.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Hl7.Fhir.OpenAPI
{
    public class Startup
    {
        private const string API_NAME = "HL7 FHIR R4 OpenAPI";
        private const string SWAGGER_ENDPOINT = "/swagger/v1.0/swagger.json";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services required for using options
            services.AddOptions();

            services.Configure<FhirOptions>(options => Configuration.GetSection("FhirOptions").Bind(options));
            services.Configure<ResourcesOptions>(options => Configuration.GetSection("ResourcesOptions").Bind(options));

            // Add the whole configuration object here
            services.AddSingleton(Configuration);

            // Configure DI for application services
            RegisterServices(services);

            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressConsumesConstraintForFormFileParameters = true;
                    options.SuppressInferBindingSourcesForParameters = true;
                    options.SuppressModelStateInvalidFilter = true; // To disable the automatic 400 behavior, set the SuppressModelStateInvalidFilter property to true
                    options.SuppressMapClientErrors = true;
                    options.ClientErrorMapping[404].Link = "https://httpstatuses.com/404";
                })
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                )
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()));

            // Add API Versioning
            // The default version is 1.0
            // And we're going to read the version number from the media type
            // Incoming requests should have a accept header like this: Accept: application/json;v=1.0
            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0); // Specify the default api version
                o.AssumeDefaultVersionWhenUnspecified = true; // Assume that the caller wants the default version if they don't specify
                o.ApiVersionReader = new MediaTypeApiVersionReader(); // Read the version number from the accept header
                o.ReportApiVersions = true; // Return Api version in response header
            });

            // Configure Swagger support
            services.ConfigureSwagger(API_NAME);

            // Configure CORS
            services.AddCorsPolicy("EnableCORS");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Need for a ReDoc logo
            var logoFilePath = "Resources/Images";
            PhysicalFileProvider fileprovider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, logoFilePath));
            var requestPath = new PathString($"/{logoFilePath}");
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = fileprovider,
                RequestPath = requestPath,
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileprovider,
                RequestPath = requestPath,
            });
            app.UseFileServer(new FileServerOptions()
            {
                FileProvider = fileprovider,
                RequestPath = requestPath,
                EnableDirectoryBrowsing = false
            });

            // ReDoc
            app.UseReDoc(sa =>
            {
                sa.DocumentTitle = $"{API_NAME} Documentation";
                sa.SpecUrl = SWAGGER_ENDPOINT;
            });

            // Swagger
            app.UseSwagger();

            // Swagger UI
            // https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-3.1&tabs=visual-studio
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(SWAGGER_ENDPOINT, $"{API_NAME} v1.0");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();
            app.UseCors("EnableCORS");
            app.UseAuthentication();

            // Request/Response logging middleware
            app.UseApiLogging();

            // Global Exception handling middleware
            app.UseGlobalExceptionHandling();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Load Citizenship list from CSV file to be a global available
            InitializeCitizenshipService(app);
        }

        /// <summary>
        /// Load Citizenship list from CSV file
        /// </summary>
        /// <param name="app"></param>
        private static void InitializeCitizenshipService(IApplicationBuilder app)
        {
            var citizenshipService = app.ApplicationServices.GetRequiredService<ICitizenshipService>();
            citizenshipService.Initialize();
        }

        protected virtual void RegisterServices(IServiceCollection services)
        {
            // Register middlewares
            services.AddTransient<ApiLogging>();
            services.AddTransient<ExceptionHandling>();

            // Services
            services.AddTransient<ICsvConverter, CsvConverter>();
            services.AddTransient<IPatientService, PatientService>();
            services.AddTransient<IObservationService, ObservationService>();
            services.AddTransient<IMedicationService, MedicationService>();
            services.AddTransient<IOrganizationService, OrganizationService>();

            services.AddSingleton<IFhirService, FhirService>();
            services.AddSingleton<ICitizenshipService, CitizenshipService>();

            // Converters
            services.AddTransient<IConverter<Patient, PatientDetailDto>, PatientToDtoConverter>();
            services.AddTransient<IConverter<IList<Patient>, IList<PatientDetailDto>>, PatientToDtoConverter>();
            services.AddTransient<IConverter<Observation, ObservationDto>, ObservationToDtoCoverter>();
            services.AddTransient<IConverter<IList<Observation>, IList<ObservationDto>>, ObservationToDtoCoverter>();
            services.AddTransient<IConverter<PatientCsv, Patient>, PatientCsvToPatientConverter>();
            services.AddTransient<IConverter<IList<PatientCsv>, IList<Patient>>, PatientCsvToPatientConverter>();
            services.AddTransient<IConverter<PatientDto, Patient>, PatientDtoToPatientConverter>();
            services.AddTransient<IConverter<IList<PatientDto>, IList<Patient>>, PatientDtoToPatientConverter>();
        }
    }
}
