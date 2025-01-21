using H1bConnect_Backend.Models.Entities.DTO;
using H1bConnect_Backend.Repository.IServices;
using H1bConnect_Backend.Repository.Services;
using Microsoft.AspNetCore.Mvc;

namespace H1bConnect_Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO signUpDTO)
        {
            try
            {
                await _authService.RegisterUserAsync(signUpDTO);
                return Ok("User registered successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                var token = await _authService.AuthenticateUserAsync(loginDTO, _configuration);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
