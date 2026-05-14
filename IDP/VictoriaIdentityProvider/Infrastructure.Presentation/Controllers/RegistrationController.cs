using Microsoft.AspNetCore.Mvc;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces;

namespace Infrastructure.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationManager _registrationManager;

        public RegistrationController(IRegistrationManager registrationManager)
        {
            _registrationManager = registrationManager;

        }
        [HttpPost(PathNames.Register)]
        public async Task<IActionResult> RegisterUserAsync(RegisterDto dto)
        {
            await _registrationManager.RegisterUserAsync(dto);

            return NoContent();
        }

        [HttpGet(PathNames.EmailConfirmation)]
        public async Task<IActionResult> ConfirmEmailAddress([FromQuery] string token)
        {
            await _registrationManager.ValidateEmailToken(token);
            return Ok(new {Message ="Your email is confimed succesfully", DateTime = DateTime.UtcNow});
        }
        [HttpPost(PathNames.ResendConfirmationLink)]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] string emailAddress) 
        {
           await _registrationManager.ResendVerificationLinkAsync(emailAddress);

            return NoContent();
        }
    }
}
