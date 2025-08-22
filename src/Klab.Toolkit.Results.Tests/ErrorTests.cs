using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Klab.Toolkit.Results.Tests;

public class ErrorTests
{
    [Fact]
    public void Create_Should_Create_Error_With_Default_Values()
    {
        // Act
        Error error = Error.Create("TEST001", "Test message");

        // Assert
        error.Code.Should().Be("TEST001");
        error.Message.Should().Be("Test message");
        error.Advice.Should().Be("");
        error.Exception.Should().BeNull();
        error.NestedErrors.Should().BeEmpty();
        error.HasNestedErrors.Should().BeFalse();
        error.TotalErrorCount.Should().Be(1);
    }

    [Fact]
    public void Create_Should_Create_Error_With_All_Parameters()
    {
        // Act
        Error error = Error.Create("TEST002", "Test message", "Test advice");

        // Assert
        error.Code.Should().Be("TEST002");
        error.Message.Should().Be("Test message");
        error.Advice.Should().Be("Test advice");
        error.Exception.Should().BeNull();
        error.NestedErrors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null, "message")]
    [InlineData("code", null)]
    public void Create_Should_Throw_ArgumentNullException_For_Null_Required_Parameters(string code, string message)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Error.Create(code, message));
    }

    [Fact]
    public void FromException_Should_Create_Error_From_Exception_With_Defaults()
    {
        // Arrange
        InvalidOperationException exception = new InvalidOperationException("Something went wrong");

        // Act
        Error error = Error.FromException(exception);

        // Assert
        error.Code.Should().Be("InvalidOperationException");
        error.Message.Should().Be("Something went wrong");
        error.Advice.Should().Be("");
        error.Exception.Should().Be(exception);
    }

    [Fact]
    public void FromException_Should_Create_Error_From_Exception_With_Custom_Parameters()
    {
        // Arrange
        ArgumentException exception = new ArgumentException("Invalid argument");

        // Act
        Error error = Error.FromException(exception, "ARG001", "Check your parameters");

        // Assert
        error.Code.Should().Be("ARG001");
        error.Message.Should().Be("Invalid argument");
        error.Advice.Should().Be("Check your parameters");
        error.Exception.Should().Be(exception);
    }

    [Fact]
    public void FromException_Should_Throw_ArgumentNullException_For_Null_Exception()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Error.FromException(null!));
    }

    [Fact]
    public void Multiple_Should_Create_Composite_Error_From_Multiple_Errors()
    {
        // Arrange
        List<Error> errors =
        [
            Error.Create("VAL001", "Name is required"),
            Error.Create("VAL002", "Email is invalid"),
            Error.Create("VAL003", "Age is below minimum")
        ];

        // Act
        Error compositeError = Error.Multiple(errors);

        // Assert
        compositeError.Code.Should().Be("MULTIPLE_ERRORS");
        compositeError.Message.Should().Be("Multiple errors occurred (3 errors)");
        compositeError.Advice.Should().Be("Review all nested errors for details");
        compositeError.HasNestedErrors.Should().BeTrue();
        compositeError.NestedErrors.Should().HaveCount(3);
        compositeError.TotalErrorCount.Should().Be(4); // 1 + 3 nested
    }

    [Fact]
    public void Multiple_Should_Return_Single_Error_When_Only_One_Error_Provided()
    {
        // Arrange
        Error singleError = Error.Create("SINGLE", "Single error");
        List<Error> errors = [singleError];

        // Act
        Error result = Error.Multiple(errors);

        // Assert
        result.Should().Be(singleError);
        result.HasNestedErrors.Should().BeFalse();
    }

    [Fact]
    public void Multiple_Should_Use_Custom_Code()
    {
        // Arrange
        List<Error> errors =
        [
            Error.Create("VAL001", "Error 1"),
            Error.Create("VAL002", "Error 2")
        ];

        // Act
        Error compositeError = Error.Multiple(errors, "CUSTOM_CODE");

        // Assert
        compositeError.Code.Should().Be("CUSTOM_CODE");
    }

    [Fact]
    public void Composite_Should_Create_Custom_Composite_Error()
    {
        // Arrange
        List<Error> errors = new List<Error>
        {
            Error.Create("VAL001", "Validation error 1"),
            Error.Create("VAL002", "Validation error 2")
        };

        // Act
        Error compositeError = Error.Composite(
            "VALIDATION_FAILED",
            "User validation failed",
            errors,
            "Please fix all validation errors");

        // Assert
        compositeError.Code.Should().Be("VALIDATION_FAILED");
        compositeError.Message.Should().Be("User validation failed");
        compositeError.Advice.Should().Be("Please fix all validation errors");
        compositeError.NestedErrors.Should().HaveCount(2);
    }

    [Fact]
    public void GetAllErrors_Should_Return_All_Errors_Flattened()
    {
        // Arrange
        List<Error> leafErrors =
        [
            Error.Create("LEAF1", "Leaf error 1"),
            Error.Create("LEAF2", "Leaf error 2")
        ];

        Error nestedError = Error.Multiple(leafErrors, "NESTED");
        Error rootError = Error.Composite("ROOT", "Root error", new[] { nestedError });

        // Act
        List<Error> allErrors = rootError.GetAllErrors().ToList();

        // Assert
        allErrors.Should().HaveCount(4); // root + nested + 2 leaf errors
        allErrors[0].Code.Should().Be("ROOT");
        allErrors[1].Code.Should().Be("NESTED");
        allErrors[2].Code.Should().Be("LEAF1");
        allErrors[3].Code.Should().Be("LEAF2");
    }

    [Fact]
    public void TotalErrorCount_Should_Count_All_Nested_Errors_Recursively()
    {
        // Arrange
        List<Error> deepestErrors =
        [
            Error.Create("DEEP1", "Deep error 1"),
            Error.Create("DEEP2", "Deep error 2")
        ];

        Error middleError = Error.Multiple(deepestErrors, "MIDDLE");
        Error rootError = Error.Composite("ROOT", "Root error", new[] { middleError });

        // Act & Assert
        rootError.TotalErrorCount.Should().Be(4); // root + middle + 2 deep errors
    }

    [Fact]
    public void ToString_Should_Return_Formatted_String()
    {
        // Arrange
        Error error = Error.Create("TEST001", "Test message", "Test advice");

        // Act
        string result = error.ToString();

        // Assert
        result.Should().Be("TEST001: Test message (Advice: Test advice)");
    }

    [Fact]
    public void ToString_Should_Include_Nested_Error_Count()
    {
        // Arrange
        List<Error> nestedErrors =
        [
            Error.Create("NESTED1", "Nested 1"),
            Error.Create("NESTED2", "Nested 2")
        ];
        Error compositeError = Error.Multiple(nestedErrors);

        // Act
        string result = compositeError.ToString();

        // Assert
        result.Should().Contain("(2 nested errors)");
    }

    [Fact]
    public void None_Should_Represent_No_Error()
    {
        // Act
        Error none = Error.None;

        // Assert
        none.Code.Should().Be("");
        none.Message.Should().Be("");
        none.Advice.Should().Be("");
        none.Exception.Should().BeNull();
        none.NestedErrors.Should().BeEmpty();
        none.HasNestedErrors.Should().BeFalse();
        none.TotalErrorCount.Should().Be(1);
    }

    [Fact]
    public void Errors_Should_Be_Immutable_Records()
    {
        // Arrange
        Error error1 = Error.Create("TEST", "Message");
        Error error2 = Error.Create("TEST", "Message");
        Error error3 = Error.Create("TEST", "Different message");

        // Assert
        error1.Should().Be(error2); // Same values should be equal
        error1.Should().NotBe(error3); // Different values should not be equal
        error1.GetHashCode().Should().Be(error2.GetHashCode()); // Same hash codes for equal records
    }
}
