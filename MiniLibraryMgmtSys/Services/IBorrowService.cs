using MiniLibraryMgmtSys.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniLibraryMgmtSys.Services
{
    public interface IBorrowService
    {
        Task<ApiResponse<BorrowResponseDto>> BorrowBookAsync(string userId, string bookId);
        Task<ApiResponse<bool>> ReturnBookAsync(string userId, string bookId);
        Task<ApiResponse<List<BorrowResponseDto>>> GetUserBorrowingHistoryAsync(string userId);
        Task<ApiResponse<List<BorrowResponseDto>>> GetAllBorrowingHistoryAsync();
    }
}
