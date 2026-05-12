using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.Domain.DTOs;
using MiniLibraryMgmtSys.Shared;
using System.Text.RegularExpressions;

namespace MiniLibraryMgmtSys.Domain.Features.User
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        public UserService(AppDbContext db)
        {
            _db = db;
        }

        private IQueryable<TblUser> ExistingUser =>
            _db.TblUsers.AsNoTracking()
            .Where(u => !u.DeleteFlag);

        private IQueryable<TblUser> ActiveUser =>
            _db.TblUsers
            .Where(u => !u.DeleteFlag);

        public async Task<bool> EmailExistsAsync(string email)
        {
            email = email.Trim().ToLower();
            return await _db.TblUsers.AnyAsync(u => u.Email == email && !u.DeleteFlag);
        }

        #region email validation
        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase,
                    TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        #endregion

        #region get all users
        public async Task<ApiResponse<List<UserResponseDTO>>> GetAllAsync()
        {
            var users = await ExistingUser
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserResponseDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    UpdatedBy = u.UpdatedBy
                })
                .ToListAsync();

            return ApiResponse<List<UserResponseDTO>>.Success(users);
        }
        #endregion

        #region get user by id
        public async Task<ApiResponse<UserResponseDTO>> GetByIdAsync(string id)
        {
            var user = await ActiveUser.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return ApiResponse<UserResponseDTO>.Failure("User not found.");

            var userDto = new UserResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                UpdatedBy = user.UpdatedBy
            };

            return ApiResponse<UserResponseDTO>.Success(userDto);
        }
        #endregion

        #region create user
        public async Task<ApiResponse<UserResponseDTO>> CreateAsync(CreateUserDTO request)
        {
            if (!IsValidEmail(request.Email)) return ApiResponse<UserResponseDTO>.Failure("Invalid email format.");

            var existingUser = await _db.TblUsers
                .AnyAsync(u => u.Email == request.Email && !u.DeleteFlag);

            if (existingUser)
            {
                return ApiResponse<UserResponseDTO>.Failure("User with this email already exists.");
            }

            string hashedPassword = PasswordHasher.Hash(request.Password);

            var newUser = new TblUser
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim(),
                Password = hashedPassword,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                DeleteFlag = false
            };

            try
            {
                _db.TblUsers.Add(newUser);
                await _db.SaveChangesAsync();

                return ApiResponse<UserResponseDTO>.Success(new UserResponseDTO { Id = newUser.Id }, "User created successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserResponseDTO>.Failure($"An error occurred during creation: {ex.Message}");
            }
        }
        #endregion

        #region update user profile
        public async Task<ApiResponse<bool>> UpdateAsync(string id, UpdateUserDTO dto, string? updatedBy = null)
        {
            var user = await ActiveUser
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return ApiResponse<bool>.Failure("User not found.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var newEmail = dto.Email.Trim().ToLower();
                if (newEmail != user.Email.ToLower() && await EmailExistsAsync(newEmail))
                    return ApiResponse<bool>.Failure("Email already exists.");

                user.Email = newEmail;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = PasswordHasher.Hash(dto.Password);

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "User updated successfully.");
        }
        #endregion

        #region soft delete user
        public async Task<ApiResponse<bool>> SoftDeleteAsync(string id, string? updatedBy = null)
        {
            var user = await ActiveUser
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return ApiResponse<bool>.Failure("User not found.");

            user.DeleteFlag = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "User deleted successfully.");
        }
        #endregion
   
    }
}
