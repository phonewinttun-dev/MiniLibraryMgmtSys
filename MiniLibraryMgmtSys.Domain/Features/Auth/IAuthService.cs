using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.Domain.DTOs;
using MiniLibraryMgmtSys.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLibraryMgmtSys.Domain.Features.Auth
{
    public interface IAuthService
    {
        Task<bool> EmailExistsAsync(string email);
        public bool IsValidEmail(string email);
        Task<TblUser?> ValidateUserAsync(string email, string password);
        Task<ApiResponse<UserResponseDTO>> RegisterAsync(RegisterDTO dto);
        Task<ApiResponse<TokenResponse>> LoginAsync(LoginDTO dto);
    }
}
