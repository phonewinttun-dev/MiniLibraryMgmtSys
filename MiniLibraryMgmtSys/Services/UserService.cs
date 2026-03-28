using Microsoft.EntityFrameworkCore;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTOs;
using MiniLibraryMgmtSys.Infrastructure;
using System.Text.RegularExpressions;

namespace MiniLibraryMgmtSys.Services
{
    public sealed class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

        private async Task<bool> EmailExistsAsync(string email)
        {
            email = email.Trim().ToLower();
            return await _db.TblUsers.AnyAsync(u => u.Email.ToLower() == email && !u.DeleteFlag);
        }

        public async Task<List<UserResponseDTO>> GetAllAsync()
        {
            return await ExistingUser
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserResponseDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();
        }

        public async Task<UserResponseDTO?> GetByIdAsync(string id)
        {
            return await ExistingUser
                .Where(u => u.Id == id)
                .Select(u => new UserResponseDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UserServiceResult<UserResponseDTO>> CreateAsync(CreateUserDTO dto)
        {
            var result = await InternalCreateAsync(dto.Name, dto.Email, dto.Password, dto.Role);
            if (!result.IsSuccess) return UserServiceResult<UserResponseDTO>.Failure(result.Message);

            var user = result.Data!;
            return UserServiceResult<UserResponseDTO>.Success(new UserResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            });
        }

        public async Task<UserServiceResult<string>> RegisterAsync(RegisterDTO dto)
        {
            var result = await InternalCreateAsync(dto.Name, dto.Email, dto.Password, dto.Role ?? UserRoles.Member);
            if (!result.IsSuccess) return UserServiceResult<string>.Failure(result.Message);

            return UserServiceResult<string>.Success(result.Data!.Id, "Registration successful");
        }

        private async Task<UserServiceResult<TblUser>> InternalCreateAsync(string name, string email, string password, string role)
        {
            email = email.Trim().ToLower();

            if (await EmailExistsAsync(email))
                return UserServiceResult<TblUser>.Failure("Email already exists.");

            if (!EmailRegex.IsMatch(email))
                return UserServiceResult<TblUser>.Failure("Invalid Email format!");

            var assignedRole = UserRoles.All.Contains(role, StringComparer.OrdinalIgnoreCase)
                ? role
                : UserRoles.Member;

            var user = new TblUser
            {
                Id = Guid.NewGuid().ToString(),
                Name = name.Trim(),
                Email = email,
                Password = PasswordHasher.Hash(password),
                Role = assignedRole,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                DeleteFlag = false
            };

            _db.TblUsers.Add(user);
            await _db.SaveChangesAsync();

            return UserServiceResult<TblUser>.Success(user);
        }

        public async Task<UserServiceResult<bool>> UpdateAsync(string id, UpdateUserDTO dto, string? updatedBy = null)
        {
            var user = await ActiveUser
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return UserServiceResult<bool>.Failure("User not found.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var newEmail = dto.Email.Trim().ToLower();
                if (newEmail != user.Email.ToLower() && await EmailExistsAsync(newEmail))
                    return UserServiceResult<bool>.Failure("Email already exists.");

                user.Email = newEmail;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = PasswordHasher.Hash(dto.Password);

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return UserServiceResult<bool>.Success(true, "User updated successfully.");
        }

        public async Task<UserServiceResult<bool>> SoftDeleteAsync(string id, string? updatedBy = null)
        {
            var user = await ActiveUser
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return UserServiceResult<bool>.Failure("User not found.");

            user.DeleteFlag = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return UserServiceResult<bool>.Success(true, "User deleted successfully.");
        }

        public async Task<TblUser?> ValidateUserAsync(string email, string password)
        {
            email = email.Trim().ToLower();

            var user = await ActiveUser
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    u.IsActive);

            if (user == null)
                return null;

            if (!PasswordHasher.Verify(password, user.Password))
                return null;

            return user;
        }
    }
}
