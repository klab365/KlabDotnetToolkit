using System;

namespace Klab.Toolkit.ValueObjects.Tests;

[TestClass]
public class ComPortTests
{
    [TestMethod]
    public void Create_ValidComPort_ReturnsComPortObject()
    {
        // Arrange
        string validComPort = "COM1";

        // Act
        ComPort comPort = ComPort.Create(validComPort);

        // Assert
        Assert.AreEqual(validComPort, comPort.Value);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("COM0")]
    [DataRow("COM257")]
    [DataRow("COMA")]
    public void Create_InvalidComPort_ThrowsArgumentException(string invalidComPort)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() => ComPort.Create(invalidComPort));
    }
}
