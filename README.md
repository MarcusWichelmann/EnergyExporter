# EnergyExporter (previously SolarEdgeExporter)

**This project was renamed and refactored to be usable for other devices than just SolarEdge.**

Collects data from Modbus devices like SolarEdge inverters and exports it as JSON, XML or to Prometheus/InfluxDB and Mqtt.

The data is queried over Modbus TCP. For now, the following devices are supported:
- SolarEdge (over SunSpec)
  - Inverters
  - Energy meters
  - Batteries (StorEdge, BYD, ...)

It's possible to query any number of devices at the same time.

## Usage

### Configuration

This exporter can be configured through commandline arguments, environment
variables ([details](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#naming-of-environment-variables)) or via a `appsettings.json`
file. The last option is probably the easiest.

You can find an example for such a configuration file at [`src/EnergyExporter/appsettings.sample.json`](src/EnergyExporter/appsettings.sample.json). This shows all available
configuration options. Just create your own `appsettings.json` based on that sample.

The `Devices` section is the main part and configures all the devices that should be queried via Modbus TCP, grouped by vendor. When multiple configured devices use the same Modbus TCP server, the TCP connection is shared between them, as many devices (e.g. SolarEdge) don't allow multiple clients to connect to Modbus TCP at the same time.

When the Modbus devices are in a master-slave configuration, the `Unit` number should be counted up for every slave. If you don't know the correct order, you can just try different options. If the exporter hangs
on start, the `Unit` number is probably wrong and it doesn't receive any response from the device.

The `InfluxDB` section in the export configuration is fully optional and can be configured to push all queried metrics directly to an InfluxDB instance. If you don't need that, just
remove that section.

Likewise, the `Mqtt` section in the export configuration is optional and can be configured to publish all queried metrics directly to an MQTT broker. If you don't need it, just
remove that section, too.


If needed, you can enable extended logging by setting the default log level in `appsettings.json` to `Debug`.

### Run the exporter

#### Using Docker

This repository provides Docker images using the GitHub Container Registry to get things running
easily: [Images](https://github.com/users/MarcusWichelmann/packages/container/package/energy-exporter)

Sample docker-compose.yml:

```yaml
energy-exporter:
    image: ghcr.io/marcuswichelmann/energy-exporter:3
    container_name: energy-exporter
    ports:
        - "14552:80"
    volumes:
        - /path/to/appsettings.json:/app/appsettings.json
    restart: always
```

#### Using .NET

You can also install the [.NET SDK](https://dotnet.microsoft.com/) on your system and run the exporter this way:

- Pull this repository and navigate into `src/EnergyExporter`
- Create an `appsettings.json` (see `appsettings.sample.json`) with your configuration options
- Execute `dotnet run -c Release`

### Access metrics

While the exporter is running and has access to the Modbus TCP interface, it will query all data for the available devices in a regular interval (`IntervalSeconds`).

Now visit the IP address and port (`14552` by default) of the running exporter to see the queried device data.

The available URLs are:

- `/metrics`: Prometheus metrics
- `/devices`: REST API (see Swagger documentation)
- `/swagger`: Interactive API documentation

The REST API will export the data in a readable JSON format. If you prefer XML, just set the HTTP `Accept` header to `application/xml` and you will get the response as XML.

## Vendors

### SolarEdge

#### Enable Modbus TCP

To enable the Modbus TCP interface on your SolarEdge inverter, enable its configuration Wifi (turn the switch on the inverter to P) and connect to it using the QR code on its side.
You don't have to use the SolarEdge app, just connect using your phone's WiFi settings. After that you can visit the IP address (the WiFi gateway IP) of the inverter and enable
Modbus TCP in the configuration interface.

You can also use the SolarEdge SetApp to enable Modbus TCP, but that will require you to be a verified installer and grant you only read-only access otherwise.
