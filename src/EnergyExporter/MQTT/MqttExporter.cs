using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EnergyExporter.Devices;
using EnergyExporter.InfluxDb;
using EnergyExporter.Options;
using EnergyExporter.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace EnergyExporter.MQTT;

public class MqttExporter : IDisposable
{
    private readonly ILogger<MqttExporter> _logger;
    private readonly IOptions<ExportOptions> _exportOptions;
    private readonly DeviceService _deviceService;

    private readonly IMqttClient? _mqttClient;
    private readonly MqttClientOptions? _mqttClientOptions;

    public MqttExporter(
        ILogger<MqttExporter> logger,
        IOptions<ExportOptions> exportOptions,
        DeviceService deviceService)
    {
        _logger = logger;
        _exportOptions = exportOptions;
        _deviceService = deviceService;

        ExportOptions.MqttOptions? mqttExportOptions = exportOptions.Value.Mqtt;
        if (mqttExportOptions != null)
        {
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(mqttExportOptions.TcpServer, mqttExportOptions.Port)
                .WithClientId(mqttExportOptions.ClientId);

            if (mqttExportOptions is { User: not null, Password: not null })
            {
                mqttClientOptions =
                    mqttClientOptions.WithCredentials(mqttExportOptions.User, mqttExportOptions.Password);
            }

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
            _mqttClientOptions = mqttClientOptions.Build();

            _mqttClient.DisconnectedAsync += args =>
            {
                _logger.LogDebug("MQTT disconnected. {Reason}", args.ReasonString);
                return Task.CompletedTask;
            };
        }
    }

    public async Task PublishMetricsAsync()
    {
        if (_mqttClient == null || _mqttClientOptions == null)
            return;

        _logger.LogDebug("Publishing metrics on MQTT");

        if (!_mqttClient.IsConnected)
        {
            _logger.LogDebug("Connecting Client");
            await _mqttClient.ConnectAsync(_mqttClientOptions);
            _logger.LogDebug("Connected Client");
        }

        // Create list of data points
        var messages =
            _deviceService.Devices.SelectMany(device => GetDeviceMeasurement(device)).ToList();

        // and publish
        foreach (var mqttApplicationMessage in messages)
        {
            await _mqttClient.PublishAsync(mqttApplicationMessage);
        }
    }

    private IEnumerable<MqttApplicationMessage> GetDeviceMeasurement(IDevice device)
    {
        var measurementAttribute = device.GetType().GetCustomAttribute<InfluxDbMeasurementAttribute>();
        if (measurementAttribute == null)
            throw new Exception($"Device {device.GetType().Name} has no measurement attribute.");

        var measurementName = measurementAttribute.Name;
        var deviceId = device.DeviceIdentifier;

        foreach (PropertyInfo property in device.GetType().GetProperties())
        {
            var attribute = property.GetCustomAttribute<InfluxDbMetricAttribute>();
            if (attribute == null)
                continue;

            object? value = property.GetValue(device);
            if (value != null && value.GetType().IsEnum)
                value = value.ToString();
            else if (value is double doubleValue)
            {
                value = doubleValue.ToString(CultureInfo.InvariantCulture);
            }
            else if (value is float floatValue)
            {
                value = floatValue.ToString(CultureInfo.InvariantCulture);
            }

            if (value != null)
            {
                yield return new MqttApplicationMessageBuilder()
                    .WithTopic(_exportOptions.Value.Mqtt!.Topic + "/" + measurementName + "/" + deviceId + "/" +
                               attribute.Field)
                    .WithPayload(value.ToString())
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
                    .WithRetainFlag(true)
                    .Build();
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _mqttClient?.DisconnectAsync();
        _mqttClient?.Dispose();
    }
}