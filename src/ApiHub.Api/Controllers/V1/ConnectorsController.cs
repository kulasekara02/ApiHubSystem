using ApiHub.Application.Features.Connectors.Commands;
using ApiHub.Application.Features.Connectors.Queries;
using ApiHub.Domain.Enums;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiHub.Api.Controllers.V1;

[ApiVersion("1.0")]
[Authorize]
public class ConnectorsController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConnectorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConnectors(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] ConnectorStatus? status = null)
    {
        var result = await Mediator.Send(new GetConnectorsQuery(pageNumber, pageSize, searchTerm, status));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateConnector([FromBody] CreateConnectorCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return CreatedAtAction(nameof(GetConnectors), new { id = result.Data }, result.Data);
    }
}
