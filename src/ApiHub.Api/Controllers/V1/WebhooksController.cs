using ApiHub.Application.Common.Interfaces;
using ApiHub.Application.Features.Webhooks.Commands;
using ApiHub.Application.Features.Webhooks.Queries;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiHub.Api.Controllers.V1;

[ApiVersion("1.0")]
[Authorize]
public class WebhooksController : BaseApiController
{
    private readonly IWebhookService _webhookService;

    public WebhooksController(IWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    /// <summary>
    /// Get all webhooks for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<WebhookDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWebhooks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isEnabled = null)
    {
        var result = await Mediator.Send(new GetWebhooksQuery(pageNumber, pageSize, searchTerm, isEnabled));
        return Ok(result);
    }

    /// <summary>
    /// Get webhook delivery history
    /// </summary>
    [HttpGet("{webhookId:guid}/deliveries")]
    [ProducesResponseType(typeof(PaginatedList<WebhookDeliveryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWebhookDeliveries(
        Guid webhookId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isSuccess = null)
    {
        var result = await Mediator.Send(new GetWebhookDeliveriesQuery(webhookId, pageNumber, pageSize, isSuccess));
        return Ok(result);
    }

    /// <summary>
    /// Create a new webhook
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWebhook([FromBody] CreateWebhookCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return CreatedAtAction(nameof(GetWebhooks), new { id = result.Data }, result.Data);
    }

    /// <summary>
    /// Update an existing webhook
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWebhook(Guid id, [FromBody] UpdateWebhookRequest request)
    {
        var command = new UpdateWebhookCommand(
            id,
            request.Name,
            request.Url,
            request.Secret,
            request.Events,
            request.IsEnabled);

        var result = await Mediator.Send(command);

        if (!result.Succeeded)
        {
            if (result.Errors.Contains("Webhook not found."))
                return NotFound(new { Error = "Webhook not found." });

            return BadRequest(new { Errors = result.Errors });
        }

        return NoContent();
    }

    /// <summary>
    /// Delete a webhook
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWebhook(Guid id)
    {
        var result = await Mediator.Send(new DeleteWebhookCommand(id));

        if (!result.Succeeded)
        {
            if (result.Errors.Contains("Webhook not found."))
                return NotFound(new { Error = "Webhook not found." });

            return BadRequest(new { Errors = result.Errors });
        }

        return NoContent();
    }

    /// <summary>
    /// Test a webhook by sending a test payload
    /// </summary>
    [HttpPost("{id:guid}/test")]
    [ProducesResponseType(typeof(TestWebhookResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestWebhook(Guid id)
    {
        var result = await Mediator.Send(new TestWebhookCommand(id));

        if (!result.Succeeded)
        {
            if (result.Errors.Contains("Webhook not found."))
                return NotFound(new { Error = "Webhook not found." });

            return BadRequest(new { Errors = result.Errors });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Retry a failed webhook delivery
    /// </summary>
    [HttpPost("deliveries/{deliveryId:guid}/retry")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RetryDelivery(Guid deliveryId)
    {
        await _webhookService.RetryDeliveryAsync(deliveryId);
        return Accepted();
    }
}

public record UpdateWebhookRequest(
    string Name,
    string Url,
    string? Secret,
    List<string> Events,
    bool IsEnabled);

public record PaginatedList<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
