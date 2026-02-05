using ApiHub.Application.Features.Reports.Commands;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiHub.Api.Controllers.V1;

[ApiVersion("1.0")]
[Authorize(Policy = "AnalystOrAdmin")]
public class ReportsController : BaseApiController
{
    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateReport([FromBody] GenerateReportCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return File(result.Data!.FileContent, result.Data.ContentType, result.Data.FileName);
    }
}
