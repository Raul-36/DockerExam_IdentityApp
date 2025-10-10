using System.Text;
using System.Text.Json;
using DockerExam_IdentityApp.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace DockerExam_IdentityApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdentityController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;

        public IdentityController(UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
        }

        [HttpPost("registration")]
        public async Task<IActionResult> Registration([FromForm] RegistrationDto dto)
        {
            var user = new IdentityUser()
            {
                Email = dto.Email,
                UserName = dto.Name,
            };
            var result = await userManager.CreateAsync(user, dto.Password);
            

            if (result.Succeeded)
            {
                var userForQueue = new
                {
                    user.Id,
                    user.UserName,
                    user.Email
                };
                PushMessage(JsonSerializer.Serialize(userForQueue), "usersQueue");
                return Ok();
            }
            else
            {
                return BadRequest(string.Join("\n", result.Errors.Select(error => error.Description)));
            }
        }

        private void PushMessage(string message, string queueName)
        {
            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST")!,
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER")!,
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASS")!
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            var messageInBytes = Encoding.ASCII.GetBytes(message);

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: queueName,
                basicProperties: null,
                body: messageInBytes
            );

            Console.WriteLine($"Push: '{message}'");
        }
    }
}
