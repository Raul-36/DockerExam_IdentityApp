using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DockerExam_IdentityApp.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

            return result.Succeeded
                ? base.RedirectToAction(actionName: "Login", controllerName: "Identity")
                : base.BadRequest(string.Join("\n", result.Errors.Select(error => error.Description)));
        }




    }
}