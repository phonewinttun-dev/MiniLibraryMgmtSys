using MiniLibraryMgmtSys.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniLibraryMgmtSys.Domain.Features.Borrow
{
    public interface IBorrowService
    {
        Task<BorrowResponseDto?> BorrowBookAsync(string userId, string bookId);
        Task<bool> ReturnBookAsync(string userId, string bookId);
        Task<List<BorrowResponseDto>> GetUserBorrowingHistoryAsync(string userId);
        Task<List<BorrowResponseDto>> GetAllBorrowingHistoryAsync();
    }
}
