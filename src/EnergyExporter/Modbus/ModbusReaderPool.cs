using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace EnergyExporter.Modbus;

public class ModbusReaderPool
{
    private record Key(string Host, ushort Port);

    private readonly ILoggerFactory _loggerFactory;

    private readonly Dictionary<Key, ModbusReader> _readers = new();
    private readonly object _lock = new();

    public ModbusReaderPool(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public ModbusReader ReaderFor(string host, ushort port)
    {
        lock (_lock)
        {
            var key = new Key(host, port);

            if (_readers.TryGetValue(key, out ModbusReader? reader))
                return reader;

            ILogger<ModbusReader> logger = _loggerFactory.CreateLogger<ModbusReader>();
            var newReader = new ModbusReader(logger, host, port);
            return _readers[key] = newReader;
        }
    }
}
