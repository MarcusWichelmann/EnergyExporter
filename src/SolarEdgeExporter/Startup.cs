using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using SolarEdgeExporter.Modbus;
using SolarEdgeExporter.Options;
using SolarEdgeExporter.Services;

namespace SolarEdgeExporter
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Register options
            services.AddOptions<ModbusOptions>().Bind(Configuration.GetSection(ModbusOptions.Key)).ValidateDataAnnotations();
            services.AddOptions<DevicesOptions>().Bind(Configuration.GetSection(DevicesOptions.Key)).ValidateDataAnnotations();
            services.AddOptions<PollingOptions>().Bind(Configuration.GetSection(PollingOptions.Key)).ValidateDataAnnotations();
            services.AddOptions<ExportOptions>().Bind(Configuration.GetSection(ExportOptions.Key)).ValidateDataAnnotations();

            var indentJson = Configuration.GetValue<bool>($"{ExportOptions.Key}:{nameof(ExportOptions.IndentedJson)}");

            // Add API support
            services.AddControllers().AddXmlSerializerFormatters().AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = indentJson);
            services.AddProblemDetails();
            services.AddSwaggerGen(options => {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "SolarEdge Exporter", Version = "v1" });
            });

            // Register services
            services.AddSingleton<ModbusReader>();
            services.AddSingleton<DeviceService>();
            services.AddHostedService<DevicePollingService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseProblemDetails();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SolarEdge Exporter v1"));

            app.UseRouting();

            app.UseEndpoints(endpoints => {
                endpoints.MapGet("/", context => context.Response.WriteAsync("SolarEdge Exporter"));
                endpoints.MapControllers();
            });
        }
    }
}
