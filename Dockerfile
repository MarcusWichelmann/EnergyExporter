FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use hardcoded amd64 SDK image: https://github.com/dotnet/dotnet-docker/issues/1537#issuecomment-755351628
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim-amd64 AS publish
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
