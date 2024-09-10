using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DockerExam_IdentityApp.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace DockerExam_IdentityApp.Controllers
{
    [Route("api/[controller]")]
    public class IdentityController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;

        public IdentityController(UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
        }
        [HttpPost]
        [Route("/api/[controller]/[action]", Name = "SignUp")]
        public async Task<IActionResult> Registration([FromForm] RegistrationDto dto)
        {
            var result = await userManager.CreateAsync(new IdentityUser()
            {
                Email = dto.Email,
                UserName = dto.Name,
            }, dto.Password);
            if(result.Succeeded){
                PushMessage(JsonSerializer.Serialize(result),"usersQueue");
                return base.Ok();
            }
            else{
                return base.BadRequest(string.Join("\n", result.Errors.Select(error => error.Description)));
            }
        }

        void PushMessage(string message, string queueName)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq_app",
                UserName = "rmuser",
                Password = "rmpassword"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var result = channel.QueueDeclare(
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

            System.Console.WriteLine($"Push: '{message}'");
        }
    }
}
