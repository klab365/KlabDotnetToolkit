using System;
using System.Net;

namespace Klab.Toolkit.ValueObjects;

/// <summary>
/// Value object which represents an IP address.
/// </summary>
public record IpAddress
{
    /// <summary>
    /// Value of the IP address.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Create a valid IP Address
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IpAddress Create(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            throw new ArgumentException("Empty IP Adress is not possible");
        }

        if (!IPAddress.TryParse(ipAddress, out IPAddress? _))
        {
            throw new ArgumentException("IP Adress is invalid");
        }

        return new IpAddress(ipAddress);
    }

    private IpAddress(string ipAddress)
    {
        Value = ipAddress;
    }
}
