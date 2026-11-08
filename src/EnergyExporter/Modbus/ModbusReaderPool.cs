using System;
using System.Collections.Generic;
using System.Linq;
using EnergyExporter.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnergyExporter.Modbus;

public sealed class ModbusReaderPool : IDisposable
{
    private record Key(string Host, ushort Port);

    private readonly ILoggerFactory _loggerFactory;
    private readonly ModbusReaderSettings _settings;

    private readonly Dictionary<Key, ModbusReader> _readers = new();
    private readonly object _lock = new();

    public ModbusReaderPool(ILoggerFactory loggerFactory, IOptions<PollingOptions> pollingOptions)
    {
        _loggerFactory = loggerFactory;
        PollingOptions options = pollingOptions.Value;
        _settings = new ModbusReaderSettings(
            TimeSpan.FromSeconds(options.ConnectTimeoutSeconds),
            TimeSpan.FromSeconds(options.ReadTimeoutSeconds),
            options.Retries);
    }

    public ModbusReader ReaderFor(string host, ushort port)
    {
        lock (_lock)
        {
            var key = new Key(host, port);

            if (_readers.TryGetValue(key, out ModbusReader? reader))
                return reader;

            ILogger<ModbusReader> logger = _loggerFactory.CreateLogger<ModbusReader>();
            return _readers[key] = new ModbusReader(logger, host, port, _settings);
        }
    }

    public void RequestReconnectAll()
    {
        lock (_lock)
        {
            foreach (ModbusReader reader in _readers.Values)
                reader.RequestReconnect();
        }
    }

    public IReadOnlyList<ModbusReader> Readers
    {
        get
        {
            lock (_lock)
            {
                return _readers.Values.ToList();
            }
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (ModbusReader reader in _readers.Values)
                reader.Dispose();
            _readers.Clear();
        }
    }
}
