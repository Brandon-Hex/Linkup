using Microsoft.AspNetCore.Mvc;
using NotificationService.NotificationRequest;

namespace NotificationService.Controllers;

[ApiController]
[Route("notifications")]
public class NotificationController : ControllerBase
{
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(ILogger<NotificationController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult SendNotification([FromBody] NotificationRequestDTO request)
    {
        if (request == null)
        {
            return BadRequest("No Notification specified.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        

        return Ok(new
        {
           // Message = $"Notification sent to {request.Email} with message '{request.Message}'."
        });
    }
}
