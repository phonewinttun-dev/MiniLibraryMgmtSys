using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.DTOs;
using MiniLibraryMgmtSys.Services;

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
            var users = await _userService.GetAllAsync();

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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invalid user data."
                });

            try
            {
                var user = await _userService.CreateAsync(dto);

                if (user == null)
                    return Conflict(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Email already exists."
                    });

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
                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred.",
                    Data = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDTO dto)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User id is required."
                });

            try
            {
                var success = await _userService.UpdateAsync(id, dto);

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
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred.",
                    Data = ex.Message
                });
            }
            
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User id is required."
                });

            try {
                var success = await _userService.SoftDeleteAsync(id);

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
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred.",
                    Data = ex.Message
                });
            }
            
        }
    }
}
