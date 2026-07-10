using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Torath.DTOs;
using Torath.Services;

namespace Torath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // Dependency Injection: We ask for the interface, the app provides the class.
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            var response = await _authService.RegisterAsync(request);

            if (!response.IsSuccess)
                return BadRequest(response.Message); // Returns HTTP 400

            return Ok(response.Message); // Returns HTTP 200
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var response = await _authService.LoginAsync(request);

            if (!response.IsSuccess)
                return Unauthorized(response.Message); // Returns HTTP 401

            // Returns HTTP 200 along with the JWT Token
            return Ok(new { token = response.Token, message = response.Message });
        }
    }
}
