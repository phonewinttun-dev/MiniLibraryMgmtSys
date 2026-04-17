using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.Domain.Features.User;
using MiniLibraryMgmtSys.DTOs;
using MiniLibraryMgmtSys.Infrastructure;
using MiniLibraryMgmtSys.Services;
using System.Security.Claims;

namespace MiniLibraryMgmtSys.Controllers
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

            var users = await _userService.GetAllAsync();

            // Access control: Librarian can only see Members
            if (currentUserRole == "Librarian")
            {
                users = users.Where(u => u.Role == "Member").ToList();
            }

            return Ok(ApiResponse<List<UserResponseDTO>>.Success(users, "Users retrieved successfully."));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(ApiResponse<object>.Failure("User id is required."));

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound(ApiResponse<object>.Failure("User not found."));

            // Access control: Librarian can only see Members
            if (currentUserRole == "Librarian" && user.Role != "Member")
            {
                return Forbid();
            }

            return Ok(ApiResponse<UserResponseDTO>.Success(user, "User retrieved successfully."));
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return NotFound(ApiResponse<object>.Failure("Profile not found."));

            return Ok(ApiResponse<UserResponseDTO>.Success(user, "Profile retrieved successfully."));
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var success = await _userService.UpdateAsync(userId, dto, userId);

                if (!success)
                    return BadRequest(ApiResponse<object>.Failure("Failed to update profile."));

                return Ok(ApiResponse<bool>.Success(true, "Profile updated successfully."));
            }
            catch (Exception ex)
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
                var user = await _userService.CreateAsync(dto);

                if (user == null)
                    return Conflict(ApiResponse<object>.Failure("Failed to create user. Email might already exist."));

                return CreatedAtAction(nameof(GetById), new { id = user.Id },
                    ApiResponse<UserResponseDTO>.Success(user, "User created successfully."));
            }
            catch (Exception ex)
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
                var success = await _userService.UpdateAsync(id, dto, currentUserId);

                if (!success)
                    return NotFound(ApiResponse<object>.Failure("User not found or update failed."));

                return Ok(ApiResponse<bool>.Success(true, "User updated successfully."));
            }
            catch (Exception ex)
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
                var success = await _userService.SoftDeleteAsync(id, currentUserId);

                if (!success)
                    return NotFound(ApiResponse<object>.Failure("User not found."));

                return Ok(ApiResponse<object>.Success(true, "User deleted successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred."));
            }
        }
    }
}
