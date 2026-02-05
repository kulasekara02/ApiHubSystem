using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Features.Auth.Commands;
using ApiHub.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace ApiHub.UnitTests.Features.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _auditServiceMock = new Mock<IAuditService>();
        _dateTimeMock = new Mock<IDateTime>();
        _dateTimeMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        _handler = new RegisterCommandHandler(
            _userManagerMock.Object,
            _auditServiceMock.Object,
            _dateTimeMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUser()
    {
        // Arrange
        var command = new RegisterCommand(
            "test@example.com",
            "Password123!",
            "John",
            "Doe");

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Viewer"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(command.Email);
        result.Data.FirstName.Should().Be(command.FirstName);
        result.Data.LastName.Should().Be(command.LastName);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCommand(
            "existing@example.com",
            "Password123!",
            "John",
            "Doe");

        var existingUser = new ApplicationUser { Email = command.Email };
        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("already exists"));
    }
}
