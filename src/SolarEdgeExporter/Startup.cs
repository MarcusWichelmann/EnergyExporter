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
            var inverter = app.ApplicationServices.GetRequiredService<ModbusReader>().ReadDevice<Inverter>(0x9C40, 109); //noch annähern
            // meter
            //var inverter = app.ApplicationServices.GetRequiredService<ModbusReader>().ReadDevice<Inverter>(0x9CB9, 123);
            // battery:
            //var inverter = app.ApplicationServices.GetRequiredService<ModbusReader>().ReadDevice<Inverter>(0xE100, 72);
            
            app.UseRouting();

            app.UseEndpoints(endpoints => {
                // TODO
            });
        }
    }
}
