using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Services.OrchestrationServices;

namespace Infrastructure.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(AuthFacade _authFacade) : ControllerBase
    {
        [HttpPost(PathNames.Login)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authFacade.LoginAsync(dto);
            return Ok(result);
        }

      
        [HttpPost(PathNames.RefreshToken)]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var dto = new TokenResultDto
            {
                JwtToken = token,
                RefreshToken = refreshToken
            };
            var result = await _authFacade.RotateTokensAsync(dto);
            return Ok(result);
        }
        [Authorize]
        [HttpPost(PathNames.Logout)]

        public async Task<IActionResult> LogoutUserAsync([FromBody] string refreshToken) 
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var dto = new TokenResultDto
            {
                JwtToken = token,
                RefreshToken= refreshToken
            };
            await _authFacade.LogoutAsync(dto);

            return NoContent();
        }
        [Authorize]
        [HttpPost(PathNames.LogoutAll)]
        public async Task<IActionResult> LogoutUserFromAllSessionsAsync([FromBody] string refreshToken)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var dto = new TokenResultDto
            {
                JwtToken = token,
                RefreshToken = refreshToken
            };
            await _authFacade.LogoutAllDevicesAsync(dto);

            return NoContent();
        }
    }
}

