using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.Domain.DTOs;
using MiniLibraryMgmtSys.Shared;

namespace MiniLibraryMgmtSys.Domain.Features.User
{
    public interface IUserService
    {
        Task<bool> EmailExistsAsync(string email);
        Task<ApiResponse<List<UserResponseDTO>>> GetAllAsync();
        Task<ApiResponse<UserResponseDTO>> GetByIdAsync(string id);
        Task<ApiResponse<UserResponseDTO>> CreateAsync(CreateUserDTO dto);
        Task<ApiResponse<bool>> UpdateAsync(string id, UpdateUserDTO dto, string? updatedBy = null);
        Task<ApiResponse<bool>> SoftDeleteAsync(string id, string? updatedBy = null);
    }
}
