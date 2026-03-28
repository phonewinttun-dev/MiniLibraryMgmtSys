using MiniLibraryMgmtSys.DTO;

namespace MiniLibraryMgmtSys.Services
{
    public interface IBookService
    {
        //Task<List<BookDto>> GetAllBooksAsync();
        Task<BookDto?> GetBookByIdAsync(string id);
        Task<List<BookDto>> GetBooksAsync();
        Task<List<BookDto>> GetAvailableBooksAsync();
        Task<List<BookDto>> SearchBooksAsync(SearchBookDto search);
        Task<BookDto?> CreateBookAsync(CreateBookDto dto);
        Task<List<BookDto>> BulkCreateBooksAsync(List<CreateBookDto> dtos);
        Task<bool> UpdateBookAsync(string id, UpdateBookDto dto);
        Task<bool> DeleteBookAsync(string id);
        Task<bool> RestoreBookAsync(string id);
        Task<bool> UpdateStatusAsync(string id, bool isAvailable);
    }
}
