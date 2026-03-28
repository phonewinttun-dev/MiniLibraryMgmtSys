using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTOs;

namespace MiniLibraryMgmtSys.Services
{
    public interface IUserService
    {
        Task<List<UserResponseDTO>> GetAllAsync();
        Task<UserResponseDTO?> GetByIdAsync(string id);
        Task<UserServiceResult<UserResponseDTO>> CreateAsync(CreateUserDTO dto);
        Task<UserServiceResult<string>> RegisterAsync(RegisterDTO dto);
        Task<UserServiceResult<bool>> UpdateAsync(string id, UpdateUserDTO dto, string? updatedBy = null);
        Task<UserServiceResult<bool>> SoftDeleteAsync(string id, string? updatedBy = null);
        Task<TblUser?> ValidateUserAsync(string email, string password);
    }
}
