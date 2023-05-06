using System.Text.Json.Serialization;
using EnergyExporter.InfluxDb;
using EnergyExporter.Modbus;
using EnergyExporter.Options;
using EnergyExporter.Prometheus;
using EnergyExporter.Services;
using EnergyExporter.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

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
        });
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options => {
        options.SwaggerDoc(
            "v1",
            new OpenApiInfo {
                Title = "Energy Exporter",
                Version = "v1"
            });
    });

// Register services
builder.Services.AddSingleton<ModbusReaderPool>();
builder.Services.AddSingleton<MetricsWriter>();
builder.Services.AddSingleton<InfluxDbExporter>();
builder.Services.AddSingleton<DeviceService>();
builder.Services.AddHostedService<DevicePollingService>();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Energy Exporter v1"));

// Info text
app.MapGet("/", context => context.Response.WriteAsync("Energy Exporter"));

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
