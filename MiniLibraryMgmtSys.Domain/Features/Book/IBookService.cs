using MiniLibraryMgmtSys.DTO;

namespace MiniLibraryMgmtSys.Domain.Features.Book
{
    public interface IBookService
    {
        //Task<List<BookDto>> GetAllBooksAsync();
        Task<BookDto?> GetBookByIdAsync(string id);
        Task<List<BookDto>> GetBooksAsync();
        Task<List<BookDto>> GetAvailableBooksAsync();
        Task<List<BookDto>> SearchBooksAsync(SearchBookDto search);
        Task<BookDto?> CreateBookAsync(CreateBookDto dto, string user);
        Task<List<BookDto>> BulkCreateBooksAsync(List<CreateBookDto> dtos, string user);
        Task<bool> UpdateBookAsync(string id, UpdateBookDto dto, string user);
        Task<bool> DeleteBookAsync(string id, string user);
        Task<bool> RestoreBookAsync(string id, string user);
        Task<bool> UpdateStatusAsync(string id, bool isAvailable, string user);
    }
}
