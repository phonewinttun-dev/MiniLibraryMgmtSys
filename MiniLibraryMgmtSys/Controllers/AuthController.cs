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
        private readonly IUserService _userService;
        private readonly GenerateJwtToken _jwtToken;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            GenerateJwtToken jwtToken,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _jwtToken = jwtToken;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration attempt for: {Email}", dto.Email);
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Failed registration attempt."
                });
            }
            
            try
            {
                _logger.LogInformation("Registration attempt for: {Email}", dto.Email);

                var userId = await _userService.RegisterAsync(dto);

                if (userId == null)
                {
                    _logger.LogWarning("Failed registration attempt for: {Email}", dto.Email);

                    return BadRequest("Email already exists.");
                }

                _logger.LogInformation("User: {UserId} registered successfully", userId);

                return Ok(new
                {
                    message = "Registration successful",
                    userId
                });
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Unexpected error during registration.");

                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occured."
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login attempt for: {Email}", dto.Email);
                
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Failed login attempt."
                });
            }

            try
            {
                _logger.LogInformation("Login attempt for: {Email}", dto.Email);

                var user = await _userService.ValidateUserAsync(dto.Email, dto.Password);

                if (user == null)
                {
                    _logger.LogWarning("Failed login attempt for: {Email}", dto.Email);

                    return Unauthorized("Invalid credentials");
                }
                var token = _jwtToken.GenerateAccessToken(user);

                _logger.LogInformation("User: {UserId} logged in successfully", user.Id);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login.");

                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred."
                });
            }
        }




    }
}
