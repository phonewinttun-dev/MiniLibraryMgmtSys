using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.Domain.DTOs;
using MiniLibraryMgmtSys.Shared;
using System.Security.Claims;

namespace MiniLibraryMgmtSys.Domain.Features.User
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> GetAll()
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var response = await _userService.GetAllAsync();

            if (response.IsSuccess && currentUserRole == "Librarian")
            {
                response.Data = response.Data?.Where(u => u.Role == "Member").ToList();
            }

            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(ApiResponse<object>.Failure("User id is required."));

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var response = await _userService.GetByIdAsync(id);

            if (!response.IsSuccess)
                return NotFound(response);

            // Access control: Librarian can only see Members
            if (currentUserRole == "Librarian" && response.Data?.Role != "Member")
            {
                return Forbid();
            }

            return Ok(response);
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var response = await _userService.GetByIdAsync(userId);
            if (!response.IsSuccess) return NotFound(response);

            return Ok(response);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var response = await _userService.UpdateAsync(userId, dto, userId);

                if (!response.IsSuccess)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred."));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateUserDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failure("Invalid user data."));

            try
            {
                var response = await _userService.CreateAsync(dto);

                if (!response.IsSuccess)
                    return Conflict(response);

                return CreatedAtAction(nameof(GetById), new { id = response.Data?.Id }, response);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred."));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDTO dto)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(ApiResponse<object>.Failure("User id is required."));

            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var response = await _userService.UpdateAsync(id, dto, currentUserId);

                if (!response.IsSuccess)
                    return NotFound(response);

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred."));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDelete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(ApiResponse<object>.Failure("User id is required."));

            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var response = await _userService.SoftDeleteAsync(id, currentUserId);

                if (!response.IsSuccess)
                    return NotFound(response);

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred."));
            }
        }
    }
}
