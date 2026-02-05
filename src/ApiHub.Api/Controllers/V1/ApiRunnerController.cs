using ApiHub.Application.Features.ApiRunner.Commands;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiHub.Api.Controllers.V1;

[ApiVersion("1.0")]
[Authorize]
public class ApiRunnerController : BaseApiController
{
    [HttpPost("send")]
    [ProducesResponseType(typeof(ApiRequestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendRequest([FromBody] SendApiRequestCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return Ok(result.Data);
    }
}
