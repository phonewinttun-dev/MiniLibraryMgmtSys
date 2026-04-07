using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
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
                _logger.LogInformation("Updating profile for user id: {UserId}", userId);

                var success = await _userService.UpdateAsync(userId, dto, userId);

                if (!success)
                    return BadRequest(ApiResponse<object>.Failure("Failed to update profile."));

                return Ok(ApiResponse<bool>.Success(true, "Profile updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating profile for user id: {UserId}", userId);
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
                _logger.LogInformation("Creating user with email: {Email}", dto.Email);

                var user = await _userService.CreateAsync(dto);

                if (user == null)
                    return Conflict(ApiResponse<object>.Failure("Failed to create user. Email might already exist."));

                _logger.LogInformation("User created with id: {UserId}", user.Id);

                return CreatedAtAction(nameof(GetById), new { id = user.Id },
                    ApiResponse<UserResponseDTO>.Success(user, "User created successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user with email: {Email}", dto.Email);

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
                _logger.LogInformation("Updating user with id: {UserId}", id);

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var success = await _userService.UpdateAsync(id, dto, currentUserId);

                if (!success)
                    return NotFound(ApiResponse<object>.Failure("User not found or update failed."));

                _logger.LogInformation("User with id: {UserId} updated successfully.", id);

                return Ok(ApiResponse<bool>.Success(true, "User updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user with id: {UserId}", id);

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
                _logger.LogInformation("Deleting user with id: {UserId}", id);

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var success = await _userService.SoftDeleteAsync(id, currentUserId);

                if (!success)
                    return NotFound(ApiResponse<object>.Failure("User not found."));

                _logger.LogInformation("User with id: {UserId} deleted successfully.", id);

                return Ok(ApiResponse<object>.Success(true, "User deleted successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user with id: {UserId}", id);

                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred."));
            }
        }
    }
}
