using Microsoft.EntityFrameworkCore;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiniLibraryMgmtSys.Domain.Features.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;
        private const int OverdueDays = 14;

        public DashboardService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ApiResponse<DashboardSummaryDto>> GetDashboardSummaryAsync()
        {
            var overdueDate = DateTime.UtcNow.AddDays(-OverdueDays);

            var totalBooks = _db.TblBooks.CountAsync(b => !b.DeleteFlag);
            var availableBooks = _db.TblBooks.CountAsync(b => !b.DeleteFlag && b.IsAvailable);
            var borrowedBooks = _db.TblBooks.CountAsync(b => !b.DeleteFlag && !b.IsAvailable);
            var totalMembers = _db.TblUsers.CountAsync(u => !u.DeleteFlag && u.Role == "Member");
            var activeBorrows = _db.TblBorrowedBooks.CountAsync(b => b.ReturnedAt == null);
            var overdueBorrows = _db.TblBorrowedBooks.CountAsync(b => b.ReturnedAt == null && b.BorrowedAt < overdueDate);

            await Task.WhenAll(
                totalBooks,
                availableBooks,
                borrowedBooks,
                totalMembers,
                activeBorrows,
                overdueBorrows
            );

            var summary = new DashboardSummaryDto
            {
                TotalBooks = await totalBooks,
                AvailableBooksCount = await availableBooks,
                BorrowedBooksCount = await borrowedBooks,
                TotalRegisteredUsersCount = await totalMembers,
                ActiveBorrowCount = await activeBorrows,
                OverdueBorrowCount = await overdueBorrows
            };

            return ApiResponse<DashboardSummaryDto>.Success(summary);
        }
    }
}
