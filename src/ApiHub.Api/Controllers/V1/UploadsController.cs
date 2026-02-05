using ApiHub.Application.Features.Uploads.Commands;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiHub.Api.Controllers.V1;

[ApiVersion("1.0")]
[Authorize]
public class UploadsController : BaseApiController
{
    [HttpPost]
    [ProducesResponseType(typeof(UploadFileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { Error = "No file uploaded" });

        using var stream = file.OpenReadStream();
        var result = await Mediator.Send(new UploadFileCommand(
            stream,
            file.FileName,
            file.ContentType,
            file.Length));

        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors });

        return Ok(result.Data);
    }
}
