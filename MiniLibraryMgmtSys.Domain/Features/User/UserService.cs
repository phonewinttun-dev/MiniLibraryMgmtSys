using Microsoft.EntityFrameworkCore;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTOs;
using MiniLibraryMgmtSys.Infrastructure;
using System.Text.RegularExpressions;

namespace MiniLibraryMgmtSys.Domain.Features.User
{
    public class UserService : IUserService
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

        public async Task<bool> EmailExistsAsync(string email)
        {
            email = email.Trim().ToLower();
            return await _db.TblUsers.AnyAsync(u => u.Email == email && !u.DeleteFlag);
        }

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

        public async Task<TblUser?> InternalCreateAsync(string name, string email, string password, string role)
        {
            email = email.Trim().ToLower();

            if (await EmailExistsAsync(email))
                return null;

            if (!EmailRegex.IsMatch(email))
                return null;


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

            return user;
        }

        public async Task<UserResponseDTO?> CreateAsync(CreateUserDTO dto)
        {
            var user = await InternalCreateAsync(dto.Name, dto.Email, dto.Password, dto.Role);
            if (user == null) return null;

            return new UserResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                UpdatedBy = user.UpdatedBy
            };
        }

        public async Task<string?> RegisterAsync(RegisterDTO dto)
        {
            var user = await InternalCreateAsync(dto.Name, dto.Email, dto.Password, dto.Role ?? UserRoles.Member);
            if (user == null) return null;

            return user.Id;
        }


        public async Task<bool> UpdateAsync(string id, UpdateUserDTO dto, string? updatedBy = null)
        {
            var user = await ActiveUser
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return false;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var newEmail = dto.Email.Trim().ToLower();
                if (newEmail != user.Email.ToLower() && await EmailExistsAsync(newEmail))
                    return false;

                user.Email = newEmail;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = PasswordHasher.Hash(dto.Password);

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(string id, string? updatedBy = null)
        {
            var user = await ActiveUser
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return false;

            user.DeleteFlag = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return true;
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
