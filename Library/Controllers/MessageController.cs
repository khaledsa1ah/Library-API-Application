using Library.DTOs;
using Library.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace Library.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class MessageController(RabbitMqService rabbitMqService) : ControllerBase
    {
        [HttpPost]
        public IActionResult PublishMessage([FromBody] MessageDto message)
        {
            if (string.IsNullOrEmpty(message.Content))
            {
                Log.Warning("Attempted to publish a message with empty content.");
                return BadRequest("Message content cannot be empty.");
            }

            Log.Information("Publishing message with content: {Content}", message.Content);
            rabbitMqService.PublishMessage(JsonSerializer.Serialize(message));
            Log.Information("Message published successfully.");

            return Ok("Message published successfully.");
        }
    }
}