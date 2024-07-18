using Day1.Dtos;
using Day1.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Day1.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class MessageController(RabbitMQService _rabbitMQService) : ControllerBase
    {
        [HttpPost]
        public IActionResult PublishMessage([FromBody] MessageDto message)
        {
            if (string.IsNullOrEmpty(message.Content))
            {
                return BadRequest("Message content cannot be empty.");
            }

            _rabbitMQService.PublishMessage(JsonSerializer.Serialize(message));
            return Ok("Message published successfully.");
        }
    }
}