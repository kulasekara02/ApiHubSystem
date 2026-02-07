using ApiHub.Application.Features.Notifications.Commands;
using ApiHub.Application.Features.Notifications.Queries;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiHub.Api.Controllers.V1;

[ApiVersion("1.0")]
[Authorize]
public class NotificationsController : BaseApiController
{
    /// <summary>
    /// Get paginated list of notifications for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isRead = null)
    {
        var result = await Mediator.Send(new GetNotificationsQuery(pageNumber, pageSize, isRead));
        return Ok(result);
    }

    /// <summary>
    /// Get the count of unread notifications for the current user
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await Mediator.Send(new GetUnreadCountQuery());
        return Ok(new { count });
    }

    /// <summary>
    /// Mark a specific notification as read
    /// </summary>
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await Mediator.Send(new MarkAsReadCommand(id));

        if (!result.Succeeded)
            return NotFound(new { Error = result.Errors.FirstOrDefault() });

        return Ok(new { message = "Notification marked as read" });
    }

    /// <summary>
    /// Mark all notifications as read for the current user
    /// </summary>
    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await Mediator.Send(new MarkAllAsReadCommand());

        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return Ok(new { message = $"{result.Data} notifications marked as read" });
    }

    /// <summary>
    /// Delete a specific notification
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await Mediator.Send(new DeleteNotificationCommand(id));

        if (!result.Succeeded)
            return NotFound(new { Error = result.Errors.FirstOrDefault() });

        return Ok(new { message = "Notification deleted" });
    }
}
