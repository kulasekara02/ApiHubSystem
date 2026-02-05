using ApiHub.Application.Features.Connectors.Commands;
using ApiHub.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace ApiHub.UnitTests.Features.Connectors;

public class CreateConnectorCommandValidatorTests
{
    private readonly CreateConnectorCommandValidator _validator;

    public CreateConnectorCommandValidatorTests()
    {
        _validator = new CreateConnectorCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateConnectorCommand(
            "Test Connector",
            "A test connector",
            "https://api.example.com",
            AuthenticationType.None,
            null,
            null,
            null,
            null,
            30,
            3,
            true,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateConnectorCommand(
            "",
            "Description",
            "https://api.example.com",
            AuthenticationType.None,
            null,
            null,
            null,
            null,
            30,
            3,
            true,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithInvalidUrl_ShouldHaveError()
    {
        // Arrange
        var command = new CreateConnectorCommand(
            "Test",
            "Description",
            "not-a-valid-url",
            AuthenticationType.None,
            null,
            null,
            null,
            null,
            30,
            3,
            true,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BaseUrl);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(301)]
    public void Validate_WithInvalidTimeout_ShouldHaveError(int timeout)
    {
        // Arrange
        var command = new CreateConnectorCommand(
            "Test",
            "Description",
            "https://api.example.com",
            AuthenticationType.None,
            null,
            null,
            null,
            null,
            timeout,
            3,
            true,
            null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TimeoutSeconds);
    }
}
