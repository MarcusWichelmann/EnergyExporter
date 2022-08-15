# SolarEdgeExporter

![GitHub Workflow Status](https://img.shields.io/github/workflow/status/MarcusWichelmann/SolarEdgeExporter/Publish%20Docker%20image?style=for-the-badge)
![GitHub](https://img.shields.io/github/license/MarcusWichelmann/SolarEdgeExporter?style=for-the-badge)
![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/MarcusWichelmann/SolarEdgeExporter?style=for-the-badge)

Collects the data of a SolarEdge installation over Modbus TCP and exports it as JSON, XML and Prometheus-Metrics. Optionally, you can also export the collected metrics to InfluxDB.

This program implements the Sunspec protocol and supports the following devices:

- Inverters
- Energy meters
- Batteries (StorEdge, BYD, ...)

It's also possible to query multiple Inverters, Meters, etc. at the same time.

## Usage

### Enable Modbus TCP

To enable the Modbus TCP interface on your SolarEdge inverter, enable its configuration Wifi (turn the switch on the inverter to P) and connect to it using the QR code on its side.
You don't have to use the SolarEdge app, just connect using your phone's WiFi settings. After that you can visit the IP address (the WiFi gateway IP) of the inverter and enable
Modbus TCP in the configuration interface.

You can also use the SolarEdge SetApp to enable Modbus TCP, but that will require you to be a verified installer and grant you only read-only access otherwise.

### Configuration

This exporter can be configured through commandline arguments, environment
variables ([details](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#naming-of-environment-variables)) or via a `appsettings.json`
file. The last option is probably the easiest.

You can find an example for such a configuration file at [`src/SolarEdgeExporter/appsettings.sample.json`](src/SolarEdgeExporter/appsettings.sample.json). This shows all available
configuration options.

The `Devices.ModbusSources` section is the main part and configures all the inverters that should be queried via Modbus TCP. When you have multiple inverters that are configured in
a master-slave setup, you should add them all here (assuming they are all connected to the network and have their own IP addresses).

The `Unit` number starts with `1` and should be counted up for every slave inverter. If you don't know the correct order, you can just try different options. If the exporter hangs
on start, the `Unit` number is probably wrong and it doesn't receive any response from the inverter.

You can also query multiple inverters, that are not realted to each other (not in a master-slave setup). In this case, you shouldn't increase the `Unit` number.

Depending on how many Modbus devices are connected to each inverter and should be queried, you should set the `Meters` and `Batteries` values accordingly.

The `InfluxDB` secion in the export configuration is fully optional and can be configured to push all queried metrics directly to an InfluxDB instance. If you don't need that, just
remove that section.

If needed, you can enable extended logging by setting the default log level in `appsettings.json` to `Debug`.

### Run the exporter

#### Using Docker

This repository provides Docker images using the GitHub Container Registry to get things running
easily: [Images](https://github.com/users/MarcusWichelmann/packages/container/package/solaredge-exporter)

Sample docker-compose.yml:

```yaml
solaredge-exporter:
    image: ghcr.io/marcuswichelmann/solaredge-exporter:2
    container_name: solaredge-exporter
    ports:
        - "14552:80"
    volumes:
        - /path/to/appsettings.json:/app/appsettings.json
    restart: always
```

#### Using .NET

You can also install the [.NET SDK](https://dotnet.microsoft.com/) on your system and run the exporter this way:

- Pull this repository and navigate into `src/SolarEdgeExporter`
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
