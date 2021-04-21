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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Serilog;

namespace Hl7.Fhir.OpenAPI
{
    public class Startup
    {
        private const string API_NAME = "HL7 FHIR R4 OpenAPI";
        private const string SWAGGER_ENDPOINT = "/swagger/v1/swagger.json";

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

            services.AddCorsPolicy("EnableCORS");
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
            services.AddApiVersioningExtension();
            services.AddSwaggerExtension(API_NAME);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Need for a ReDoc logo
            const string LOGO_FILE_PATH = "Resources/Images";
            var fileprovider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, LOGO_FILE_PATH));
            var requestPath = new PathString($"/{LOGO_FILE_PATH}");
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

            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();

            // For elevated security, it is recommended to remove this middleware and set your server to only listen on https. 
            // A slightly less secure option would be to redirect http to 400, 505, etc.
            app.UseHttpsRedirection();

            app.UseCors("EnableCORS");

            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseErrorHandlingMiddleware();
            
            // Request/Response logging middleware
            app.UseApiLogging();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Dynamic App
            app.UseSwaggerExtension();

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
            services.AddTransient<ErrorHandlerMiddleware>();

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
