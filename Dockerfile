# Build on the native platform and cross-compile for the target architecture.
# This avoids running the .NET SDK under QEMU emulation, which is slow and unreliable on arm64.
# Defaults for the legacy (non-BuildKit) builder; BuildKit/buildx set these automatically.
ARG BUILDPLATFORM=linux/amd64
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS publish

ARG TARGETARCH=amd64
ARG TARGETOS=linux
RUN arch=$TARGETARCH \
    && if [ "$arch" = "amd64" ]; then arch="x64"; fi \
    && echo $TARGETOS-$arch > /tmp/rid \
    && cat /tmp/rid

COPY src/EnergyExporter/EnergyExporter.csproj /src/EnergyExporter/
WORKDIR /src/EnergyExporter
RUN dotnet restore EnergyExporter.csproj -r $(cat /tmp/rid)

COPY src/EnergyExporter/ .
RUN dotnet publish EnergyExporter.csproj -r $(cat /tmp/rid) -c Release -o /app/publish --no-self-contained --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnergyExporter.dll"]
