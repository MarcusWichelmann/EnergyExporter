# SolarEdgeExporter

![GitHub Workflow Status](https://img.shields.io/github/workflow/status/MarcusWichelmann/SolarEdgeExporter/Publish%20Docker%20image?style=for-the-badge)
![GitHub](https://img.shields.io/github/license/MarcusWichelmann/SolarEdgeExporter?style=for-the-badge)
![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/MarcusWichelmann/SolarEdgeExporter?style=for-the-badge)

Collects the data of a SolarEdge installation over Modbus TCP and exports it as JSON, XML and Prometheus-Metrics.

It implements the Sunspec protocol and supports the following devices:
- Inverters
- Energy meters
- Batteries (StorEdge, BYD, ...)

## Usage

### Enable Modbus TCP

To enable the Modbus TCP interface on your SolarEdge inverter, enable its configuration Wifi (turn the switch on the inverter to P) and connect to it using the QR code on its side. After that you can visit the IP address (gateway IP) of the inverter and enable Modbus TCP in the configuration interface. (You can also use the SolarEdge SetApp, but this one will require you to be an installer and grant you read-only access only otherwise.)

### Run the exporter

#### Using Docker

This repository provides Docker images using the GitHub Container Registry to get things running easily: [Images](https://github.com/users/MarcusWichelmann/packages/container/package/solaredge-exporter)

Sample docker-compose.yml:

```yaml
solaredge-exporter:
  image: ghcr.io/marcuswichelmann/solaredge-exporter:1
  container_name: solaredge-exporter
  ports:
    - "14552:80"
  environment:
    - "Modbus__Host=10.16.100.8"
    - "Modbus__Port=1502"
    - "Devices__Inverters=1"
    - "Devices__Meters=1"
    - "Devices__Batteries=1"
    - "Polling__IntervalSeconds=10"
    - "Export__IndentedJson=true"
  restart: always
```

See `src/SolarEdgeExporter/appsettings.sample.json` for all available configuration options.

#### Using .NET

You can also install the [.NET SDK](https://dotnet.microsoft.com/) on your system and run the exporter this way:
- Pull this repository and navigate into `src/SolarEdgeExporter`
- Create an `appsettings.json` (see `appsettings.sample.json`) with your configuration options
- Execute `dotnet run -c Release`

If needed, you can enable extended logging by setting the default log level in `appsettings.json` to `Debug`.

### Access metrics

While the exporter is running and has access to the Modbus TCP interface, it will query all data for the available devices in a regular interval (`IntervalSeconds`).

You can specify which devices are available by setting the `Inverters`, `Meters` and `Batteries` configuraiton options accordingly. Set `Batteries` to 0 if no battery is connected to your solar installation.

Now visit the IP address and port of the running exporter to see the queried device data (may take a few seconds to show up, depending on your choosen inverval).

The available URLs are:
- `/metrics`: Prometheus metrics
- `/devices`: REST API (see Swagger documentation)
- `/swagger`: Interactive API documentation

The REST API will export the data in a readable JSON format. If you prefer XML, just set the HTTP `Accept` header to `application/xml` and you will get the response as XML.
