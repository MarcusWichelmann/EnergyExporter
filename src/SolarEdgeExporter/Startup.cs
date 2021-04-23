using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarEdgeExporter.Devices;
using SolarEdgeExporter.Modbus;
using SolarEdgeExporter.Options;

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
            services.AddOptions<SolarEdgeOptions>().Bind(Configuration.GetSection("SolarEdge"))
                .ValidateDataAnnotations();

            // Register services
            services.AddSingleton<ModbusReader>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // TODO Test code
            // inverter:
            //var inverter = app.ApplicationServices.GetRequiredService<ModbusReader>().ReadDevice<Inverter>(0x9C40, 109);
            // meter
            //var meter = app.ApplicationServices.GetRequiredService<ModbusReader>().ReadDevice<Meter>(0x9CB9, 174);
            // battery:
            var battery = app.ApplicationServices.GetRequiredService<ModbusReader>().ReadDevice<Battery>(0xE100, 158);

            app.UseRouting();

            app.UseEndpoints(endpoints => {
                // TODO
            });
        }
    }
}
