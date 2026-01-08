using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.DTOs;
using MiniLibraryMgmtSys.Services;

namespace MiniLibraryMgmtSys.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Users retrieved successfully.",
                Data = users
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User id is required."
                });

            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User not found."
                });

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "User retrieved successfully.",
                Data = user
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invalid user data."
                });

            var user = await _userService.CreateUserAsync(dto);

            if (user == null)
                return Conflict(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Email already exists."
                });

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id },
                new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "User created successfully.",
                    Data = user
                });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDTO dto)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User id is required."
                });

            var success = await _userService.UpdateUserAsync(id, dto);

            if (!success)
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User not found."
                });

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "User updated successfully."
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User id is required."
                });

            var success = await _userService.SoftDeleteUserAsync(id);

            if (!success)
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User not found."
                });

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "User deleted successfully."
            });
        }
    }
}
