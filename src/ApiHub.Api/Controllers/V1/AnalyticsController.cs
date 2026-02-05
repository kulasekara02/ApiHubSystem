using ApiHub.Application.Features.Analytics.Queries;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiHub.Api.Controllers.V1;

[ApiVersion("1.0")]
[Authorize(Policy = "AnalystOrAdmin")]
public class AnalyticsController : BaseApiController
{
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardData), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? connectorId = null)
    {
        var result = await Mediator.Send(new GetDashboardDataQuery(fromDate, toDate, connectorId));
        return Ok(result);
    }
}
