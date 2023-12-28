namespace Klab.Toolkit.ValueObjects.Tests;

[TestClass]
public class CurrentTests
{
    [TestMethod]
    public void Create_ValidCurrent_ReturnsCurrentObject()
    {
        // Arrange
        double validAmpere = 2.5;

        // Act
        Current current = Current.Create(validAmpere);

        // Assert
        Assert.AreEqual(validAmpere, current.Ampere);
    }

    [TestMethod]
    public void FromMilliAmpere_ValidMilliAmpere_ReturnsCurrentObject()
    {
        // Arrange
        double validMilliAmpere = 2500;
        double expectedAmpere = 2.5;

        // Act
        Current current = Current.FromMilliAmpere(validMilliAmpere);

        // Assert
        Assert.AreEqual(expectedAmpere, current.Ampere);
    }

    [TestMethod]
    public void MilliAmpere_ValidCurrent_ReturnsMilliAmpereValue()
    {
        // Arrange
        double validAmpere = 2.5;
        double expectedMilliAmpere = 2500;

        // Act
        Current current = Current.Create(validAmpere);

        // Assert
        Assert.AreEqual(expectedMilliAmpere, current.MilliAmpere);
    }
}
