using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTOs;

namespace MiniLibraryMgmtSys.Services
{
    public interface IUserService
    {
        Task<List<UserResponseDTO>> GetAllAsync();
        Task<UserResponseDTO?> GetByIdAsync(string id);
        Task<TblUser?> CreateAsync(CreateUserDTO dto);
        Task<string?> RegisterAsync(RegisterDTO dto);
        Task<bool> UpdateAsync(string id, UpdateUserDTO dto, string? updatedBy = null);
        Task<bool> SoftDeleteAsync(string id, string? updatedBy = null);
        Task<TblUser?> ValidateUserAsync(string email, string password);

    }
}
