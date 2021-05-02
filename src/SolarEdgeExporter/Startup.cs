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
            services.AddOptions<ModbusOptions>().Bind(Configuration.GetSection("Modbus")).ValidateDataAnnotations();
            services.AddOptions<DevicesOptions>().Bind(Configuration.GetSection("Devices")).ValidateDataAnnotations();
            services.AddOptions<PollingOptions>().Bind(Configuration.GetSection("Polling")).ValidateDataAnnotations();

            // Add API support
            services.AddControllers().AddXmlSerializerFormatters();
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
