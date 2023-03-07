using Microsoft.VisualStudio.TestTools.UnitTesting;
using Klab.Toolkit.Common.String;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace Klab.Toolkit.Common.String.Tests
{
    [TestClass()]
    public class StringExtensionsTests
    {
        [TestMethod()]
        public void IsEmail_ReturnsTrue_ForValidEmailAddress()
        {
            // Arrange
            string email = "johndoe@example.com";

            // Act
            bool result = email.IsEmail();

            // Assert
            _ = result.Should().BeTrue();
        }

        [TestMethod()]
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
}