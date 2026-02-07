using ApiHub.Application.Features.ScheduledJobs.Commands;
using ApiHub.Application.Features.ScheduledJobs.Queries;
using ApiHub.Infrastructure.BackgroundJobs;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiHub.Api.Controllers.V1;

[ApiVersion("1.0")]
[Authorize]
public class ScheduledJobsController : BaseApiController
{
    private readonly IScheduledJobService _scheduledJobService;

    public ScheduledJobsController(IScheduledJobService scheduledJobService)
    {
        _scheduledJobService = scheduledJobService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ScheduledJobDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScheduledJobs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isEnabled = null,
        [FromQuery] Guid? connectorId = null)
    {
        var result = await Mediator.Send(new GetScheduledJobsQuery(
            pageNumber, pageSize, searchTerm, isEnabled, connectorId));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ScheduledJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetScheduledJob(Guid id)
    {
        var result = await Mediator.Send(new GetScheduledJobsQuery(
            PageNumber: 1, PageSize: 1, SearchTerm: null, IsEnabled: null, ConnectorId: null));

        var job = result.Items.FirstOrDefault(j => j.Id == id);
        if (job == null)
            return NotFound();

        return Ok(job);
    }

    [HttpGet("{id:guid}/executions")]
    [ProducesResponseType(typeof(IEnumerable<JobExecutionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobExecutionHistory(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var result = await Mediator.Send(new GetJobExecutionHistoryQuery(
            id, pageNumber, pageSize, status));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateScheduledJob([FromBody] CreateScheduledJobCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        // Register job with Hangfire if enabled
        if (command.IsEnabled)
        {
            await _scheduledJobService.RegisterJobAsync(result.Data);
        }

        return CreatedAtAction(nameof(GetScheduledJob), new { id = result.Data }, result.Data);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateScheduledJob(Guid id, [FromBody] UpdateScheduledJobCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { Error = "ID mismatch" });

        var result = await Mediator.Send(command);

        if (!result.Succeeded)
        {
            if (result.Errors.Contains("Scheduled job not found"))
                return NotFound();
            return BadRequest(new { Errors = result.Errors });
        }

        // Update job registration with Hangfire
        await _scheduledJobService.RegisterJobAsync(id);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteScheduledJob(Guid id)
    {
        // Unregister from Hangfire first
        await _scheduledJobService.UnregisterJobAsync(id);

        var result = await Mediator.Send(new DeleteScheduledJobCommand(id));

        if (!result.Succeeded)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id:guid}/toggle")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleJob(Guid id, [FromBody] ToggleJobRequest request)
    {
        var result = await Mediator.Send(new ToggleJobCommand(id, request.IsEnabled));

        if (!result.Succeeded)
        {
            if (result.Errors.Contains("Scheduled job not found"))
                return NotFound();
            return BadRequest(new { Errors = result.Errors });
        }

        // Update job registration with Hangfire
        await _scheduledJobService.RegisterJobAsync(id);

        return NoContent();
    }

    [HttpPost("{id:guid}/run-now")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RunJobNow(Guid id)
    {
        // Execute job immediately (in background)
        await _scheduledJobService.ExecuteJobAsync(id);
        return Accepted();
    }

    public record ToggleJobRequest(bool IsEnabled);
}
