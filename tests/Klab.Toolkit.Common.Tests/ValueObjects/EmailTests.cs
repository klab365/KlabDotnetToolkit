using FluentAssertions;
using Klab.Toolkit.Common.ValueObjects;

namespace Klab.Toolkit.CommonTests.ValueObjects;

[TestClass]
public class EmailTests
{
    [TestMethod]
    public void IsEmail_ReturnsTrue_ForValidEmailAddress()
    {
        // Arrange
        string emailInput = "johndoe@example.com";

        // Act
        Email email = Email.Create(emailInput);

        // assert
        email.Value.Should().Be(emailInput);
    }

    [TestMethod]
    public void IsEmail_ReturnsFalse_ForInvalidEmailAddress()
    {
        // Arrange
        string email = "johndoeexample.com";

        // Act & assert
        Action act = () => Email.Create(email);
        act.Should().Throw<ArgumentException>()
            .WithMessage("E-Mail is invalid");
    }
}
