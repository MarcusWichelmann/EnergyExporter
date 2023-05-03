using System;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SolarEdgeExporter.InfluxDb;
using SolarEdgeExporter.Options;
using SolarEdgeExporter.Prometheus;
using SolarEdgeExporter.Services;
using SolarEdgeExporter.Utils;

WebApplicationBuilder builder = WebApplication.CreateBuilder();

builder.Host.UseSystemd();

// Register options
builder.Services.AddOptions<DevicesOptions>().Bind(builder.Configuration.GetSection(DevicesOptions.Key))
    .RecursivelyValidateDataAnnotations();
builder.Services.AddOptions<PollingOptions>().Bind(builder.Configuration.GetSection(PollingOptions.Key))
    .RecursivelyValidateDataAnnotations();
builder.Services.AddOptions<ExportOptions>().Bind(builder.Configuration.GetSection(ExportOptions.Key))
    .RecursivelyValidateDataAnnotations();

var indentJson = builder.Configuration.GetValue<bool>($"{ExportOptions.Key}:{nameof(ExportOptions.IndentedJson)}");

// Add API support
builder.Services.AddControllers().AddXmlSerializerFormatters().AddJsonOptions(options => {
    options.JsonSerializerOptions.WriteIndented = indentJson;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SolarEdge Exporter", Version = "v1" });
});

// Register services
builder.Services.AddSingleton<MetricsWriter>();
builder.Services.AddSingleton<InfluxDbExporter>();
builder.Services.AddSingleton<DeviceService>();
builder.Services.AddHostedService<DevicePollingService>();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SolarEdge Exporter v1"));

// Info text
app.MapGet("/", context => context.Response.WriteAsync("SolarEdge Exporter"));

// API routes
app.MapControllers();

// Metrics endpoint
app.MapGet("/metrics", context => {
    context.Response.ContentType = "text/plain";
    var metricsWriter = context.RequestServices.GetRequiredService<MetricsWriter>();
    return metricsWriter.WriteToStreamAsync(context.Response.Body);
});

app.Run();
