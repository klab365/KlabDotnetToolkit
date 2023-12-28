using System;

namespace Klab.Toolkit.ValueObjects.Tests;

[TestClass]
public class IpAddressTest
{
    [TestMethod]
    public void Create_ValidIpAddress_ReturnsIpAddressObject()
    {
        // Arrange
        string validIpAddress = "192.168.0.1";

        // Act
        IpAddress ipAddress = IpAddress.Create(validIpAddress);

        // Assert
        Assert.AreEqual(validIpAddress, ipAddress.Value);
    }

    [TestMethod]
    public void Create_EmptyIpAddress_ThrowsArgumentException()
    {
        // Arrange
        string emptyIpAddress = "";

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => IpAddress.Create(emptyIpAddress));
    }

    [TestMethod]
    public void Create_InvalidIpAddress_ThrowsArgumentException()
    {
        // Arrange
        string invalidIpAddress = "not_an_ip_address";

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => IpAddress.Create(invalidIpAddress));
    }
}
