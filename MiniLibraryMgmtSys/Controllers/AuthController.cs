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
        private readonly IJwtTokenService _jwtToken;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            IJwtTokenService jwtToken,
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
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invalid input data."
                });
            }

            try
            {
                _logger.LogInformation("Registration attempt for: {Email}", dto.Email);

                var result = await _userService.RegisterAsync(dto);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed registration attempt for: {Email}. Reason: {Reason}", dto.Email, result.Message);

                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = result.Message
                    });
                }

                _logger.LogInformation("User: {UserId} registered successfully", result.Data);

                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "Registration successful",
                    Data = new { userId = result.Data }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration.");

                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred."
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invalid input data."
                });
            }

            try
            {
                _logger.LogInformation("Login attempt for: {Email}", dto.Email);

                var user = await _userService.ValidateUserAsync(dto.Email, dto.Password);

                if (user == null)
                {
                    _logger.LogWarning("Failed login attempt for: {Email}", dto.Email);

                    return Unauthorized(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Invalid credentials"
                    });
                }

                var token = _jwtToken.GenerateAccessToken(user);

                _logger.LogInformation("User: {UserId} logged in successfully", user.Id);

                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "Login successful",
                    Data = new
                    {
                        token,
                        user = new
                        {
                            user.Id,
                            user.Name,
                            user.Email,
                            user.Role
                        }
                    }
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
