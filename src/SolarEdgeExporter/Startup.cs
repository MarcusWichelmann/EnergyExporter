using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarEdgeExporter.Devices;
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

            // Register services
            services.AddSingleton<ModbusReader>();
            services.AddSingleton<DeviceService>();
            services.AddHostedService<DevicePollingService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // TODO Test code
            // inverter:
            //var inverter = app.ApplicationServices.GetRequiredService<ModbusReader>().ReadDevice<Inverter>(0x9C40);
            // meter
            //var meter = app.ApplicationServices.GetRequiredService<ModbusReader>().ReadDevice<Meter>(0x9CB9);
            // battery:
            //var battery = app.ApplicationServices.GetRequiredService<ModbusReader>().ReadDeviceAsync<Battery>(0xE100).Result;

            app.UseRouting();

            app.UseEndpoints(endpoints => {
                // TODO
            });
        }
    }
}
