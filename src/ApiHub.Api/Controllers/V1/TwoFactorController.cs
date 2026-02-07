using ApiHub.Application.Features.Auth.Commands.Enable2FA;
using ApiHub.Application.Features.Auth.Queries;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiHub.Api.Controllers.V1;

[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/2fa")]
public class TwoFactorController : BaseApiController
{
    /// <summary>
    /// Start 2FA setup - generates secret key and QR code URI
    /// </summary>
    [HttpPost("setup")]
    [ProducesResponseType(typeof(Enable2FAResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Setup()
    {
        var result = await Mediator.Send(new Enable2FACommand());

        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Verify TOTP code and enable 2FA
    /// </summary>
    [HttpPost("verify")]
    [ProducesResponseType(typeof(Verify2FAResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Verify([FromBody] Verify2FARequest request)
    {
        var result = await Mediator.Send(new Verify2FACommand(request.Code));

        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Disable 2FA with password confirmation
    /// </summary>
    [HttpPost("disable")]
    [ProducesResponseType(typeof(Disable2FAResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Disable([FromBody] Disable2FARequest request)
    {
        var result = await Mediator.Send(new Disable2FACommand(request.Password));

        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get 2FA status for the current user
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(Get2FAStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatus()
    {
        var result = await Mediator.Send(new Get2FAStatusQuery());

        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return Ok(result.Data);
    }
}

public record Verify2FARequest(string Code);
public record Disable2FARequest(string Password);
