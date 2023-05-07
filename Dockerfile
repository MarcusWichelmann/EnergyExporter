# The .NET SDK is broken on arm64, so we have to cross-compile instead.
FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim-amd64 AS publish

# Workaround from: https://github.com/dotnet/sdk/issues/28971#issuecomment-1308881150
ARG TARGETARCH
ARG TARGETOS
RUN arch=$TARGETARCH \
    && if [ "$arch" = "amd64" ]; then arch="x64"; fi \
    && echo $TARGETOS-$arch > /tmp/rid \
    && cat /tmp/rid

COPY src/EnergyExporter/EnergyExporter.csproj /src/EnergyExporter/
WORKDIR /src/EnergyExporter
RUN dotnet restore EnergyExporter.csproj -r $(cat /tmp/rid)

COPY src/EnergyExporter/ .
RUN dotnet publish EnergyExporter.csproj -r $(cat /tmp/rid) -c Release -o /app/publish --no-self-contained --no-restore

FROM  mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnergyExporter.dll"]
