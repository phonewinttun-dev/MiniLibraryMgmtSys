using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.DTOs;
using MiniLibraryMgmtSys.Infrastructure;
using MiniLibraryMgmtSys.Services;


namespace MiniLibraryMgmtSys.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly GenerateJwtToken _jwtToken;

        public AuthController(
            UserService userService,
            GenerateJwtToken jwtToken)
        {
            _userService = userService;
            _jwtToken = jwtToken;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            var userId = await _userService.RegisterUserAsync(dto);

            if (userId == null)
                return BadRequest("Email already exists.");

            return Ok(new
            {
                message = "Registration successful",
                userId
            });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var user = await _userService.ValidateUserAsync(dto.Email, dto.Password);

            if (user == null)
                return Unauthorized("Invalid credentials");

            var token = _jwtToken.GenerateAccessToken(user);

            return Ok(new
            {
                message = "Login successful",
                token,
                user.Id,
                user.Name,
                user.Email,
                user.Role
            });
        }

    }


}
