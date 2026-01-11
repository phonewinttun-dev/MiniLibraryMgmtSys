using Azure.Core;
using Microsoft.EntityFrameworkCore;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTO;
using MiniLibraryMgmtSys.DTOs;
using MiniLibraryMgmtSys.Infrastructure;
using System.Runtime.CompilerServices;

namespace MiniLibraryMgmtSys.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        private IQueryable<TblUser> ExistingUser =>
            _db.TblUsers.AsNoTracking()
            .Where(u => !u.DeleteFlag);

        //private IQueryable<TblUser> DuplicateEmail(string email) =>
        //    _db.TblUsers.AsNoTracking()
        //    .Where(u => u.Email == email);


        //check if email exists
        private async Task<bool> EmailExistsAsync(string email)
        {
            email = email.Trim().ToLower();

            return await ExistingUser.AnyAsync(u =>
                u.Email.ToLower() == email);
        }

        public async Task<List<UserResponseDTO>> GetAllUsersAsync()
        {
            return await ExistingUser
                .AsNoTracking()
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
                .AsNoTracking()
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

        public async Task<TblUser?> CreateUserAsync(CreateUserDTO dto)
        {
            // Prevent duplicate email
            var emailExists = await EmailExistsAsync(dto.Email);

            if (emailExists)
                return null;

            var allowedRoles = new[] { "Member", "Admin", "Librarian" };
            var role = string.IsNullOrWhiteSpace(dto.Role) ? "Member" : dto.Role;
            if (!allowedRoles.Contains(role))
                role = "Member";

            var user = new TblUser
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Email = dto.Email.Trim().ToLower(),
                Password = PasswordHasher.Hash(dto.Password),
                Role = role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true, 
                DeleteFlag = false
            };

            _db.TblUsers.Add(user);
            await _db.SaveChangesAsync();

            return user;
        }

        public async Task<string?> RegisterUserAsync(RegisterDTO dto)
        {

            // Prevent duplicate email
            var emailExists = await EmailExistsAsync(dto.Email);

            if (emailExists)
                return null;
            
            var allowedRoles = new[] { "Member", "Admin", "Librarian" };
            var role = string.IsNullOrWhiteSpace(dto.Role) ? "Member" : dto.Role;
            if (!allowedRoles.Contains(role))
                role = "Member";

            var user = new TblUser
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim().ToLower(),
                Password = PasswordHasher.Hash(dto.Password),
                Role = role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                DeleteFlag = false
            };

            _db.TblUsers.Add(user);
            await _db.SaveChangesAsync();

            return user.Id;
        }

        public async Task<bool> UpdateUserAsync(string id, UpdateUserDTO dto, string? updatedBy = null)
        {
            var user = await ExistingUser
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return false;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Email))
                if (await EmailExistsAsync(dto.Email) && dto.Email.Trim().ToLower() != user.Email.ToLower())
                    return false;
                else
                    user.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = dto.Password;

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteUserAsync(string id, string? updatedBy = null)
        {
            var user = await ExistingUser
                .FirstOrDefaultAsync(u => u.Id == id && !u.DeleteFlag);

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

            var user = await _db.TblUsers
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    !u.DeleteFlag &&
                    u.IsActive);

            if (user == null)
                return null;

            if (!PasswordHasher.Verify(password, user.Password))
                return null;

            return user;
        }
    }
}
