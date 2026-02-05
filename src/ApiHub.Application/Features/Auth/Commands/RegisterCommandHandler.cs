using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Common.Models;
using ApiHub.Domain.Entities;
using ApiHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ApiHub.Application.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;
    private readonly IDateTime _dateTime;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IAuditService auditService,
        IDateTime dateTime)
    {
        _userManager = userManager;
        _auditService = auditService;
        _dateTime = dateTime;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result<RegisterResponse>.Failure("A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = _dateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return Result<RegisterResponse>.Failure(errors);
        }

        // Assign default Viewer role
        await _userManager.AddToRoleAsync(user, "Viewer");

        await _auditService.LogAsync(
            AuditAction.Register,
            nameof(ApplicationUser),
            user.Id.ToString(),
            newValues: new { user.Email, user.FirstName, user.LastName },
            cancellationToken: cancellationToken);

        return Result<RegisterResponse>.Success(new RegisterResponse(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName));
    }
}
