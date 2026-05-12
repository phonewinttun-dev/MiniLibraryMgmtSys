using MiniLibraryMgmtSys.Domain.DTOs;
using MiniLibraryMgmtSys.Shared;

namespace MiniLibraryMgmtSys.Domain.Features.Book
{
    public interface IBookService
    {
        Task<ApiResponse<BookDto>> GetBookByIdAsync(string id);
        Task<ApiResponse<List<BookDto>>> GetBooksAsync();
        Task<ApiResponse<List<BookDto>>> GetAvailableBooksAsync();
        Task<ApiResponse<List<BookDto>>> SearchBooksAsync(SearchBookDto search);
        Task<ApiResponse<BookDto>> CreateBookAsync(CreateBookDto dto, string user);
        Task<ApiResponse<List<BookDto>>> BulkCreateBooksAsync(List<CreateBookDto> dtos, string user);
        Task<ApiResponse<bool>> UpdateBookAsync(string id, UpdateBookDto dto, string user);
        Task<ApiResponse<bool>> DeleteBookAsync(string id, string user);
        Task<ApiResponse<bool>> RestoreBookAsync(string id, string user);
        Task<ApiResponse<bool>> UpdateStatusAsync(string id, bool isAvailable, string user);
    }
}
