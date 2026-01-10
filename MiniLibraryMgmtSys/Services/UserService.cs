using Azure.Core;
using Microsoft.EntityFrameworkCore;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTO;
using MiniLibraryMgmtSys.DTOs;

namespace MiniLibraryMgmtSys.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<UserResponseDTO>> GetAllUsersAsync()
        {
            return await _db.TblUsers
                .AsNoTracking()
                .Where(u => !u.DeleteFlag)
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserResponseDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                })
                .ToListAsync();
        }

        public async Task<TblUser?> GetByIdAsync(string id)
        {
            return await _db.TblUsers
                .AsNoTracking()
                .Where(u => u.Id == id && !u.DeleteFlag)
                .Select(u => new TblUser
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TblUser?> CreateUserAsync(CreateUserDTO dto, string? createdBy = null)
        {
            // Prevent duplicate email
            bool emailExists = await _db.TblUsers
                .AnyAsync(u => u.Email == dto.Email && !u.DeleteFlag);

            if (emailExists)
                return null;

            var user = new TblUser
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                UpdatedBy = createdBy,
                DeleteFlag = false
            };

            _db.TblUsers.Add(user);
            await _db.SaveChangesAsync();

            return user;
        }

        public async Task<bool> UpdateUserAsync(string id, UpdateUserDTO dto, string? updatedBy = null)
        {
            var user = await _db.TblUsers
                .FirstOrDefaultAsync(u => u.Id == id && !u.DeleteFlag);

            if (user == null)
                return false;

            if (!string.IsNullOrEmpty(dto.Name))
                user.Name = dto.Name;

            if (!string.IsNullOrEmpty(dto.Email))
                user.Email = dto.Email;

            if (!string.IsNullOrEmpty(dto.Password))
                user.Password = dto.Password;

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteUserAsync(string id, string? updatedBy = null)
        {
            var user = await _db.TblUsers
                .FirstOrDefaultAsync(u => u.Id == id && !u.DeleteFlag);

            if (user == null)
                return false;

            user.DeleteFlag = true;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = updatedBy;

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
