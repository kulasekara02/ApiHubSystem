using ApiHub.Application.Features.Records.Queries;
using ApiHub.Domain.Enums;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiHub.Api.Controllers.V1;

[ApiVersion("1.0")]
[Authorize]
public class RecordsController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ApiRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecords(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? connectorId = null,
        [FromQuery] HttpMethodType? method = null,
        [FromQuery] int? statusCode = null,
        [FromQuery] bool? isSuccess = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? searchTerm = null)
    {
        var result = await Mediator.Send(new GetApiRecordsQuery(
            pageNumber, pageSize, connectorId, method, statusCode,
            isSuccess, fromDate, toDate, searchTerm));

        return Ok(result);
    }
}
