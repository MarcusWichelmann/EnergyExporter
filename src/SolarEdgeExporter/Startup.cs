using System.Text.Json.Serialization;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using SolarEdgeExporter.Modbus;
using SolarEdgeExporter.Options;
using SolarEdgeExporter.Prometheus;
using SolarEdgeExporter.Services;
using SolarEdgeExporter.Utils;

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
            services.AddOptions<DevicesOptions>().Bind(Configuration.GetSection(DevicesOptions.Key)).RecursivelyValidateDataAnnotations();
            services.AddOptions<PollingOptions>().Bind(Configuration.GetSection(PollingOptions.Key)).RecursivelyValidateDataAnnotations();
            services.AddOptions<ExportOptions>().Bind(Configuration.GetSection(ExportOptions.Key)).RecursivelyValidateDataAnnotations();

            var indentJson = Configuration.GetValue<bool>($"{ExportOptions.Key}:{nameof(ExportOptions.IndentedJson)}");

            // Add API support
            services.AddControllers().AddXmlSerializerFormatters().AddJsonOptions(options => {
                options.JsonSerializerOptions.WriteIndented = indentJson;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.AddProblemDetails();
            services.AddSwaggerGen(options => {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "SolarEdge Exporter", Version = "v1" });
            });

            // Register services
            services.AddSingleton<MetricsWriter>();
            services.AddSingleton<DeviceService>();
            services.AddHostedService<DevicePollingService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, MetricsWriter metricsWriter)
        {
            app.UseProblemDetails();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SolarEdge Exporter v1"));

            app.UseRouting();

            app.UseEndpoints(endpoints => {
                // Info text
                endpoints.MapGet("/", context => context.Response.WriteAsync("SolarEdge Exporter"));

                // API routes
                endpoints.MapControllers();

                // Metrics endpoint
                endpoints.MapGet("/metrics", context => {
                    context.Response.ContentType = "text/plain";
                    return metricsWriter.WriteToStreamAsync(context.Response.Body);
                });
            });
        }
    }
}
