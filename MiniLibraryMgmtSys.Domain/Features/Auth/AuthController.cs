using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.Domain.Features.Auth;
using MiniLibraryMgmtSys.Domain.Features.User;
using MiniLibraryMgmtSys.DTOs;
using MiniLibraryMgmtSys.Infrastructure;

namespace MiniLibraryMgmtSys.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtToken;

        public AuthController(
            IUserService userService,
            IJwtTokenService jwtToken)
        {
            _userService = userService;
            _jwtToken = jwtToken;
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
                var userId = await _userService.RegisterAsync(dto);

                if (userId == null)
                {
                    return BadRequest(ApiResponse<object>.Failure("Registration failed. Email might already exist."));
                }

                return Ok(ApiResponse<object>.Success(new { userId }, "Registration successful"));
            }
            catch (Exception ex)
            {
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
                var user = await _userService.ValidateUserAsync(dto.Email, dto.Password);

                if (user == null)
                {
                    return Unauthorized(ApiResponse<object>.Failure("Invalid credentials"));
                }

                var token = _jwtToken.GenerateAccessToken(user);

                return Ok(ApiResponse<object>.Success(new
                {
                    token,
                    user = new
                    {
                        user.Id,
                        user.Name,
                        user.Email,
                        user.Role
                    }
                }, "Login successful"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred."
                });
            }
        }
    }
}
