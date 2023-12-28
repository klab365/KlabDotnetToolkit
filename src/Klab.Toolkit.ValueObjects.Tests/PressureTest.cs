namespace Klab.Toolkit.ValueObjects.Tests;

[TestClass]
public class PressureTest
{
    [TestMethod]
    public void FromMilliBar_ValidValue_ReturnsPressureObject()
    {
        // Arrange
        double validValue = 100.0;

        // Act
        Pressure pressure = Pressure.FromMilliBar(validValue);

        // Assert
        Assert.AreEqual(validValue * Pressure.PASCAL_TO_MBAR, pressure.Pascal);
    }

    [TestMethod]
    public void FromBar_ValidValue_ReturnsPressureObject()
    {
        // Arrange
        double validValue = 1.0;

        // Act
        Pressure pressure = Pressure.FromBar(validValue);

        // Assert
        Assert.AreEqual(validValue * Pressure.PASCAL_TO_BAR, pressure.Pascal);
    }

    [TestMethod]
    public void MilliBar_ValidValue_ReturnsCorrectValue()
    {
        // Arrange
        double pascalValue = 1000.0;
        Pressure pressure = Pressure.FromPascal(pascalValue);

        // Act
        double milliBarValue = pressure.MilliBar;

        // Assert
        Assert.AreEqual(pascalValue / Pressure.PASCAL_TO_MBAR, milliBarValue);
    }

    [TestMethod]
    public void Bar_ValidValue_ReturnsCorrectValue()
    {
        // Arrange
        double pascalValue = 100000.0;
        Pressure pressure = Pressure.FromPascal(pascalValue);

        // Act
        double barValue = pressure.Bar;

        // Assert
        Assert.AreEqual(pascalValue / Pressure.PASCAL_TO_BAR, barValue);
    }
}
