using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexusIMS.API.DTOs.UserDTOs;
using NexusIMS.API.Services;

namespace NexusIMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
                return BadRequest("يجب ادخال البيانات");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authService.RegisterAsync(registerDto);
            if(!result.IsSuccess)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto) 
        {
            if (loginDto == null)
                return BadRequest("يجب ادخال البيانات");

            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);
            if (!result.IsSuccess) return Unauthorized(result);

            return Ok(result);
        }
    }
}
