using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTOs;
using MiniLibraryMgmtSys.Infrastructure;

namespace MiniLibraryMgmtSys.Domain.Features.User
{
    public interface IUserService
    {
        Task<bool> EmailExistsAsync(string email);
        Task<ApiResponse<List<UserResponseDTO>>> GetAllAsync();
        Task<ApiResponse<UserResponseDTO>> GetByIdAsync(string id);
        Task<TblUser?> InternalCreateAsync(string name, string email, string password, string role);
        Task<UserResponseDTO?> CreateAsync(CreateUserDTO dto);
        Task<string?> RegisterAsync(RegisterDTO dto);
        Task<bool> UpdateAsync(string id, UpdateUserDTO dto, string? updatedBy = null);
        Task<bool> SoftDeleteAsync(string id, string? updatedBy = null);
        Task<TblUser?> ValidateUserAsync(string email, string password);
    }
}
