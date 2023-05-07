FROM mcr.microsoft.com/dotnet/sdk:7.0 AS publish
WORKDIR /src
COPY src/EnergyExporter/EnergyExporter.csproj EnergyExporter/
WORKDIR /src/EnergyExporter
RUN dotnet restore EnergyExporter.csproj
COPY src/EnergyExporter/ .
RUN dotnet publish EnergyExporter.csproj -c Release -o /app/publish

FROM  mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnergyExporter.dll"]
