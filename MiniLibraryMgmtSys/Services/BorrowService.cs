using Microsoft.EntityFrameworkCore;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniLibraryMgmtSys.Services
{
    public class BorrowService : IBorrowService
    {
        private readonly AppDbContext _db;
        private const int DefaultBorrowDays = 14;

        public BorrowService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ApiResponse<BorrowResponseDto>> BorrowBookAsync(string userId, string bookId)
        {
            var book = await _db.TblBooks.FirstOrDefaultAsync(b => b.Id == bookId && !b.DeleteFlag);
            if (book == null)
                return new ApiResponse<BorrowResponseDto> { IsSuccess = false, Message = "Book not found." };

            if (!book.IsAvailable)
                return new ApiResponse<BorrowResponseDto> { IsSuccess = false, Message = "Book is already borrowed." };

            var user = await _db.TblUsers.FirstOrDefaultAsync(u => u.Id == userId && !u.DeleteFlag);
            if (user == null)
                return new ApiResponse<BorrowResponseDto> { IsSuccess = false, Message = "User not found." };

            // Prevent double borrow of the same book by the same user if not returned
            var existingBorrow = await _db.TblBorrowedBooks
                .AnyAsync(bb => bb.UserId == userId && bb.BookId == bookId && bb.ReturnedAt == null);

            if (existingBorrow)
                return new ApiResponse<BorrowResponseDto> { IsSuccess = false, Message = "You already have this book borrowed." };

            var borrowRecord = new TblBorrowedBook
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                BookId = bookId,
                BorrowedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            book.IsAvailable = false;
            book.UpdatedAt = DateTime.UtcNow;
            book.UpdatedBy = userId;

            _db.TblBorrowedBooks.Add(borrowRecord);
            await _db.SaveChangesAsync();

            return new ApiResponse<BorrowResponseDto>
            {
                IsSuccess = true,
                Message = "Book borrowed successfully.",
                Data = MapToDto(borrowRecord, book, user)
            };
        }

        public async Task<ApiResponse<bool>> ReturnBookAsync(string userId, string bookId)
        {
            var borrowRecord = await _db.TblBorrowedBooks
                .Include(bb => bb.Book)
                .FirstOrDefaultAsync(bb => bb.UserId == userId && bb.BookId == bookId && bb.ReturnedAt == null);

            if (borrowRecord == null)
                return new ApiResponse<bool> { IsSuccess = false, Message = "No active borrow record found for this book and user." };

            borrowRecord.ReturnedAt = DateTime.UtcNow;
            borrowRecord.UpdatedAt = DateTime.UtcNow;

            borrowRecord.Book.IsAvailable = true;
            borrowRecord.Book.UpdatedAt = DateTime.UtcNow;
            borrowRecord.Book.UpdatedBy = userId;

            await _db.SaveChangesAsync();

            return new ApiResponse<bool> { IsSuccess = true, Message = "Book returned successfully.", Data = true };
        }

        public async Task<ApiResponse<List<BorrowResponseDto>>> GetUserBorrowingHistoryAsync(string userId)
        {
            var history = await _db.TblBorrowedBooks
                .Include(bb => bb.Book)
                .Include(bb => bb.User)
                .Where(bb => bb.UserId == userId)
                .OrderByDescending(bb => bb.BorrowedAt)
                .Select(bb => MapToDto(bb, bb.Book, bb.User))
                .ToListAsync();

            return new ApiResponse<List<BorrowResponseDto>>
            {
                IsSuccess = true,
                Message = "Borrowing history retrieved successfully.",
                Data = history
            };
        }

        public async Task<ApiResponse<List<BorrowResponseDto>>> GetAllBorrowingHistoryAsync()
        {
            var history = await _db.TblBorrowedBooks
                .Include(bb => bb.Book)
                .Include(bb => bb.User)
                .OrderByDescending(bb => bb.BorrowedAt)
                .Select(bb => MapToDto(bb, bb.Book, bb.User))
                .ToListAsync();

            return new ApiResponse<List<BorrowResponseDto>>
            {
                IsSuccess = true,
                Message = "All borrowing history retrieved successfully.",
                Data = history
            };
        }

        private static BorrowResponseDto MapToDto(TblBorrowedBook bb, TblBook book, TblUser user)
        {
            var isOverdue = bb.ReturnedAt == null && (DateTime.UtcNow - bb.BorrowedAt).TotalDays > DefaultBorrowDays;

            return new BorrowResponseDto
            {
                Id = bb.Id,
                UserId = bb.UserId,
                UserName = user.Name,
                BookId = bb.BookId,
                BookTitle = book.Title,
                BookAuthor = book.Author,
                BorrowedAt = bb.BorrowedAt,
                ReturnedAt = bb.ReturnedAt,
                IsOverdue = isOverdue
            };
        }
    }
}
