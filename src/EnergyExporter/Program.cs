using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EnergyExporter.InfluxDb;
using EnergyExporter.Modbus;
using EnergyExporter.Mqtt;
using EnergyExporter.Options;
using EnergyExporter.Prometheus;
using EnergyExporter.Services;
using EnergyExporter.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

WebApplicationBuilder builder = WebApplication.CreateBuilder();

builder.Host.UseSystemd();

// Register options
builder.Services.AddOptions<DevicesOptions>()
    .Bind(builder.Configuration.GetSection(DevicesOptions.Key))
    .RecursivelyValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<PollingOptions>()
    .Bind(builder.Configuration.GetSection(PollingOptions.Key))
    .RecursivelyValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<ExportOptions>()
    .Bind(builder.Configuration.GetSection(ExportOptions.Key))
    .RecursivelyValidateDataAnnotations()
    .ValidateOnStart();

var indentJson = builder.Configuration.GetValue<bool>($"{ExportOptions.Key}:{nameof(ExportOptions.IndentedJson)}");

// Add API support
builder.Services.AddControllers()
    .AddXmlSerializerFormatters()
    .AddJsonOptions(
        options => {
            options.JsonSerializerOptions.WriteIndented = indentJson;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.Converters.Add(new NaNAsNullDoubleConverter());
        });
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(
    options => options.AddDocumentTransformer(
        (document, _, _) => {
            document.Info.Title = "Energy Exporter";
            document.Info.Version = "v1";
            return Task.CompletedTask;
        }));

// Register services
builder.Services.AddSingleton<ModbusReaderPool>();
builder.Services.AddSingleton<MetricsWriter>();
builder.Services.AddSingleton<InfluxDbExporter>();
builder.Services.AddSingleton<MqttExporter>();
builder.Services.AddSingleton<StatusEventLog>();
builder.Services.AddSingleton<DeviceService>();
builder.Services.AddSingleton<DevicePollingService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DevicePollingService>());

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// API description at /openapi/v1.json, interactive docs at /swagger
app.MapOpenApi();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/openapi/v1.json", "Energy Exporter v1"));

// Management UI (wwwroot/index.html)
app.UseDefaultFiles();
app.UseStaticFiles();

// API routes
app.MapControllers();

// Metrics endpoint
app.MapGet(
    "/metrics",
    context => {
        context.Response.ContentType = "text/plain";
        var metricsWriter = context.RequestServices.GetRequiredService<MetricsWriter>();
        return metricsWriter.WriteToStreamAsync(context.Response.Body);
    });

app.Run();
