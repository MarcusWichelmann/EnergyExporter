using SolarEdgeExporter.Modbus;
using SolarEdgeExporter.Prometheus;

namespace SolarEdgeExporter.Devices
{
    public class Inverter : IDevice
    {
        [StringModbusRegister(4, 32)]
        public string? Manufacturer { get; init; }

        [StringModbusRegister(20, 32)]
        public string? Model { get; init; }

        [StringModbusRegister(44, 16)]
        public string? Version { get; init; }

        [StringModbusRegister(52, 32)]
        public string? SerialNumber { get; init; }

        [ModbusRegister(68)]
        public ushort DeviceAddress { get; init; }

        [ModbusRegister(69)]
        public ushort SunSpecDID { get; init; }

        [ScaledModbusRegister(71, typeof(ushort), 75, typeof(short))]
        public double AC_Current { get; init; }

        [ScaledModbusRegister(72, typeof(ushort), 75, typeof(short))]
        public double AC_CurrentA { get; init; }

        [ScaledModbusRegister(73, typeof(ushort), 75, typeof(short))]
        public double AC_CurrentB { get; init; }

        [ScaledModbusRegister(74, typeof(ushort), 75, typeof(short))]
        public double AC_CurrentC { get; init; }

        [ScaledModbusRegister(76, typeof(ushort), 82, typeof(short))]
        public double AC_VoltageAB { get; init; }

        [ScaledModbusRegister(77, typeof(ushort), 82, typeof(short))]
        public double AC_VoltageBC { get; init; }

        [ScaledModbusRegister(78, typeof(ushort), 82, typeof(short))]
        public double AC_VoltageCA { get; init; }

        [ScaledModbusRegister(79, typeof(ushort), 82, typeof(short))]
        public double AC_VoltageAN { get; init; }

        [ScaledModbusRegister(80, typeof(ushort), 82, typeof(short))]
        public double AC_VoltageBN { get; init; }

        [ScaledModbusRegister(81, typeof(ushort), 82, typeof(short))]
        public double AC_VoltageCN { get; init; }

        [ScaledModbusRegister(83, typeof(short), 84, typeof(short))]
        public double AC_Power { get; init; }

        [ScaledModbusRegister(85, typeof(ushort), 86, typeof(short))]
        public double AC_Frequency { get; init; }

        [ScaledModbusRegister(87, typeof(short), 88, typeof(short))]
        public double AC_VA { get; init; }

        [ScaledModbusRegister(89, typeof(short), 90, typeof(short))]
        public double AC_VAR { get; init; }

        [ScaledModbusRegister(91, typeof(short), 92, typeof(short))]
        public double AC_PF { get; init; }

        [ScaledModbusRegister(93, typeof(uint), 95, typeof(ushort))]
        public double AC_Energy_WH { get; init; }

        [ScaledModbusRegister(96, typeof(ushort), 97, typeof(short))]
        public double DC_Current { get; init; }

        [ScaledModbusRegister(98, typeof(ushort), 99, typeof(short))]
        public double DC_Voltage { get; init; }

        [ScaledModbusRegister(100, typeof(short), 101, typeof(short))]
        public double DC_Power { get; init; }

        [ScaledModbusRegister(103, typeof(short), 106, typeof(short))]
        public double Temp_Sink { get; init; }

        [ModbusRegister(107)]
        public ushort Status { get; init; }

        [ModbusRegister(108)]
        public ushort Status_Vendor { get; init; }
    }
}
