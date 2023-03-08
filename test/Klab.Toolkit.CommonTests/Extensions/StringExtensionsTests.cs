using FluentAssertions;
using Klab.Toolkit.Common.String;

namespace Klab.Toolkit.Common.Extensions.Tests;

[TestClass]
public class StringExtensionsTests
{
    [TestMethod]
    public void IsEmail_ReturnsTrue_ForValidEmailAddress()
    {
        // Arrange
        string email = "johndoe@example.com";

        // Act
        bool result = email.IsEmail();

        // Assert
        _ = result.Should().BeTrue();
    }

    [TestMethod]
    public void IsEmail_ReturnsFalse_ForInvalidEmailAddress()
    {
        // Arrange
        string email = "johndoeexample.com";

        // Act
        bool result = email.IsEmail();

        // Assert
        _ = result.Should().BeFalse();
    }
}
