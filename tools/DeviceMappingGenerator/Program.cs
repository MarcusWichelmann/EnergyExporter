using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DeviceMappingGenerator;

public static class Program
{
    // A text file that contains the raw data from the tables in the SunSpec PDFs
    private const string FileName = "MeterRegisters.txt";

    private static readonly Regex LineRegex = new("^([0-9]+) [0-9]+ ([0-9]+) ([^ ]+) ([^ ]+) (.*)$");

    public static void Main(string[] args)
    {
        // Parse registers
        Register[] registers = File.ReadAllLines(FileName)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(
                line => {
                    string[] values = LineRegex.Match(line).Groups.Values.Select(v => v.Value).ToArray();
                    return new Register(int.Parse(values[1]), int.Parse(values[2]), values[3], values[4], values[5]);
                })
            .ToArray();

        // Find start offset
        int startOffset = registers.Min(r => r.Address);

        foreach (Register register in registers)
        {
            if (register.Name.EndsWith("_SF"))
                continue;

            Register? scaleFactorRegister = registers.FirstOrDefault(r => r.Name == $"{register.Name}_SF");

            // Map to C# type
            string propertyType;
            var stringLength = 0;
            if (register.Type.StartsWith("String("))
            {
                propertyType = "string?";
                stringLength = int.Parse(register.Type[7..^1]);
            }
            else
            {
                propertyType = register.Type switch {
                    "uint16" => "ushort",
                    "int16" => "short",
                    "uint32" => "uint",
                    "int32" => "int",
                    "acc32" => "uint",
                    _ => throw new Exception($"Unexpected register type: {register.Type}")
                };
            }

            if (scaleFactorRegister != null)
            {
                string scaleFactorRegisterType = scaleFactorRegister.Type switch {
                    "uint16" => "ushort",
                    "int16" => "short",
                    _ => throw new Exception($"Unexpected scale factor register type: {register.Type}")
                };

                Console.WriteLine(
                    $"[ScaledModbusRegister({register.Address - startOffset}, typeof({propertyType}), {scaleFactorRegister.Address - startOffset}, typeof({scaleFactorRegisterType}))]");
                propertyType = "double";
            }
            else if (propertyType == "string?")
            {
                Console.WriteLine($"[StringModbusRegister({register.Address - startOffset}, {stringLength})]");
            }
            else
            {
                Console.WriteLine($"[ModbusRegister({register.Address - startOffset})]");
            }

            string registerName = register.Name;
            if (registerName.StartsWith("C_") || registerName.StartsWith("I_") || registerName.StartsWith("M_"))
                registerName = registerName[2..];

            Console.WriteLine($"public {propertyType} {registerName} {{ get; init; }}");
            Console.WriteLine();
        }
    }
}

public class Register
{
    public int Address { get; }
    public int Size { get; }
    public string Name { get; }
    public string Type { get; }
    public string Comment { get; }

    public Register(int address, int size, string name, string type, string comment)
    {
        Address = address;
        Size = size;
        Name = name;
        Type = type;
        Comment = comment;
    }
}
