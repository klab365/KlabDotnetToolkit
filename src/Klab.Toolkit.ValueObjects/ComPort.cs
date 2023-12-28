using System;

namespace Klab.Toolkit.ValueObjects;

/// <summary>
/// Value object which represent a COM Port
/// </summary>
public record ComPort
{
    private const string ComPortRegexPattern = @"^COM([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-6])$";

    /// <summary>
    /// COM Port itself
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Create a valid COM Port
    /// </summary>
    /// <param name="comPort"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static ComPort Create(string comPort)
    {
        if (string.IsNullOrWhiteSpace(comPort))
        {
            throw new ArgumentException("Empty COM Port is not possible");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(comPort, ComPortRegexPattern))
        {
            throw new ArgumentException("COM Port is invalid");
        }

        return new ComPort(comPort);
    }

    private ComPort(string comPort)
    {
        Value = comPort;
    }
}
