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
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failure("Invalid input data."));
            
            try
            {
                var response = await _authService.RegisterAsync(dto);

                if (!response.IsSuccess) return BadRequest(response);
                
                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred."));
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failure("Invalid input data."));

            try
            {
                var response = await _authService.LoginAsync(dto);

                if (!response.IsSuccess) return Unauthorized(response);

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred."));
            }
        }
    }
}
