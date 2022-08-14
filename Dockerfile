FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0 AS publish
WORKDIR /src
COPY src/SolarEdgeExporter/SolarEdgeExporter.csproj SolarEdgeExporter/
WORKDIR /src/SolarEdgeExporter
RUN dotnet restore SolarEdgeExporter.csproj
COPY src/SolarEdgeExporter/ .
RUN dotnet publish SolarEdgeExporter.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SolarEdgeExporter.dll"]
