using ApiHub.Application.Common.Models;
using ApiHub.Application.Features.Templates.Commands;
using ApiHub.Application.Features.Templates.Queries;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiHub.Api.Controllers.V1;

[ApiVersion("1.0")]
[Authorize]
public class TemplatesController : BaseApiController
{
    /// <summary>
    /// Get paginated list of request templates
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<TemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isPublic = null,
        [FromQuery] Guid? connectorId = null,
        [FromQuery] bool includePrivate = false)
    {
        var result = await Mediator.Send(new GetTemplatesQuery(
            pageNumber, pageSize, searchTerm, isPublic, connectorId, includePrivate));
        return Ok(result);
    }

    /// <summary>
    /// Get a specific request template by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplate(Guid id)
    {
        var result = await Mediator.Send(new GetTemplateByIdQuery(id));

        if (!result.Succeeded)
            return NotFound(new { Error = result.Errors.FirstOrDefault() });

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new request template
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return CreatedAtAction(nameof(GetTemplate), new { id = result.Data }, result.Data);
    }

    /// <summary>
    /// Update an existing request template
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] UpdateTemplateRequest request)
    {
        var command = new UpdateTemplateCommand(
            id,
            request.Name,
            request.Description,
            request.ConnectorId,
            request.Method,
            request.Endpoint,
            request.Headers,
            request.Body,
            request.QueryParams,
            request.IsPublic);

        var result = await Mediator.Send(command);

        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Contains("not found")))
                return NotFound(new { Error = result.Errors.FirstOrDefault() });
            return BadRequest(new { Errors = result.Errors });
        }

        return NoContent();
    }

    /// <summary>
    /// Delete a request template
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        var result = await Mediator.Send(new DeleteTemplateCommand(id));

        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Contains("not found")))
                return NotFound(new { Error = result.Errors.FirstOrDefault() });
            return BadRequest(new { Errors = result.Errors });
        }

        return NoContent();
    }
}

/// <summary>
/// Request model for updating a template (without ID which comes from route)
/// </summary>
public record UpdateTemplateRequest(
    string Name,
    string? Description,
    Guid? ConnectorId,
    string Method,
    string Endpoint,
    string? Headers,
    string? Body,
    string? QueryParams,
    bool IsPublic);
