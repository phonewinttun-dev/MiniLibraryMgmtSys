using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.DTOs;
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

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Users retrieved successfully.",
                Data = users
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User id is required."
                });

            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User not found."
                });

            // Access control: Librarian can only see Members
            if (currentUserRole == "Librarian" && user.Role != "Member")
            {
                return Forbid();
            }

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "User retrieved successfully.",
                Data = user
            });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return NotFound(new ApiResponse<object> { IsSuccess = false, Message = "Profile not found." });

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Profile retrieved successfully.",
                Data = user
            });
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

                // Prevent members from updating their own role via profile update if the DTO allows it

                var result = await _userService.UpdateAsync(userId, dto, userId);

                if (!result.IsSuccess)
                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = result.Message
                    });

                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "Profile updated successfully.",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating profile for user id: {UserId}", userId);
                return StatusCode(500, new ApiResponse<object> { IsSuccess = false, Message = "An unexpected error occurred." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateUserDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invalid user data."
                });

            try
            {
                _logger.LogInformation("Creating user with email: {Email}", dto.Email);

                var result = await _userService.CreateAsync(dto);

                if (!result.IsSuccess)
                    return Conflict(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = result.Message
                    });

                var user = result.Data!;
                _logger.LogInformation("User created with id: {UserId}", user.Id);

                return CreatedAtAction(nameof(GetById), new { id = user.Id },
                    new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Message = "User created successfully.",
                        Data = user
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user with email: {Email}", dto.Email);

                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred."
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDTO dto)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User id is required."
                });

            try
            {
                _logger.LogInformation("Updating user with id: {UserId}", id);

                var user = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var result = await _userService.UpdateAsync(id, dto, user);

                if (!result.IsSuccess)
                    return NotFound(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = result.Message
                    });

                _logger.LogInformation("User with id: {UserId} updated successfully.", id);

                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "User updated successfully.",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user with id: {UserId}", id);

                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred."
                });
            }

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDelete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User id is required."
                });

            try
            {
                _logger.LogInformation("Deleting user with id: {UserId}", id);

                var user = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var result = await _userService.SoftDeleteAsync(id, user);

                if (!result.IsSuccess)
                return NotFound(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = result.Message
                    });

                _logger.LogInformation("User with id: {UserId} deleted successfully.", id);

                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "User deleted successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user with id: {UserId}", id);

                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred."
                });
            }

        }
    }
}
