using ApiHub.Application.Features.Connectors.Queries;
using ApiHub.Domain.Enums;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiHub.Api.Controllers.V2;

[ApiVersion("2.0")]
[Authorize]
public class ConnectorsController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(ConnectorsV2Response), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConnectors(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] ConnectorStatus? status = null)
    {
        var result = await Mediator.Send(new GetConnectorsQuery(pageNumber, pageSize, searchTerm, status));

        // V2 returns additional metadata
        var response = new ConnectorsV2Response
        {
            Data = result.Items,
            Pagination = new PaginationInfo
            {
                CurrentPage = result.PageNumber,
                TotalPages = result.TotalPages,
                TotalCount = result.TotalCount,
                HasNext = result.HasNextPage,
                HasPrevious = result.HasPreviousPage
            },
            ApiVersion = "2.0"
        };

        return Ok(response);
    }
}

public class ConnectorsV2Response
{
    public IEnumerable<ConnectorDto> Data { get; set; } = new List<ConnectorDto>();
    public PaginationInfo Pagination { get; set; } = new();
    public string ApiVersion { get; set; } = string.Empty;
}

public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}
