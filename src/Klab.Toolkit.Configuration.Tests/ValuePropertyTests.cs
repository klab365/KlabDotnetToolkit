using System;
using System.Collections.Generic;

namespace Klab.Toolkit.Configuration.Tests;

[TestClass]
public class PropertyTests
{
    [TestMethod]
    public void TestPropertyWithValidCallbacks()
    {
        // Arrange
        List<Func<int, bool>> callbacks = new()
        {
            x => x > 0,
            x => x % 2 == 0,
        };
        ValueProperty<int> prop = new("MyProperty", 2, callbacks);

        // Act
        bool isValid = prop.IsValid();

        // Assert
        Assert.IsTrue(isValid);
    }

    [TestMethod]
    public void TestPropertyWithoutValidCallbacks()
    {
        // Arrange
        ValueProperty<int> prop = new("MyProperty", 2);

        // Act
        bool isValid = prop.IsValid();

        // Assert
        Assert.IsTrue(isValid);
    }

    [TestMethod]
    public void TestPropertyWithInvalidCallbacks()
    {
        // Arrange
        List<Func<int, bool>> callbacks = new()
        {
            x => x > 0,
            x => x % 2 == 0,
        };
        ValueProperty<int> prop = new("MyProoperty", -1, callbacks);

        // Act
        bool isValid = prop.IsValid();

        // Assert
        Assert.IsFalse(isValid);
    }
}
