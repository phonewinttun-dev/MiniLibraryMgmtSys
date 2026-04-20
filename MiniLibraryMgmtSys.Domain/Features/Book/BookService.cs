using Microsoft.EntityFrameworkCore;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTO;
using MiniLibraryMgmtSys.Domain.Features.Book;
using MiniLibraryMgmtSys.Infrastructure;

namespace MiniLibraryMgmtSys.Domain.Features.Book
{
    public class BookService : IBookService
    {
        private readonly AppDbContext _db;

        public BookService(AppDbContext db)
        {
            _db = db;
        }

        private IQueryable<TblBook> ActiveBooks =>
        _db.TblBooks.Where(b => !b.DeleteFlag);

        private IQueryable<TblBook> AvailableBooks =>
            ActiveBooks.Where(b => b.IsAvailable);


        // get all books including deleted books
        // public async Task<List<BookDto>> GetAllBooksAsync()
        // {
        //     return await _db.TblBooks
        //         .AsNoTracking()
        //         .Select(b => new BookDto
        //         {
        //             Id = b.Id,
        //             Author = b.Author,
        //             Title = b.Title,
        //             Genre = b.Genre,
        //             IsAvailable = b.IsAvailable,
        //             DeleteFlag = b.DeleteFlag
        //         })
        //         .ToListAsync();
        // }

        public async Task<ApiResponse<BookDto>> GetBookByIdAsync(string id)
        {
            var book = await ActiveBooks
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return ApiResponse<BookDto>.Failure("Book not found.");

            var bookDto = new BookDto
            {
                Id = book.Id,
                Author = book.Author,
                Title = book.Title,
                Genre = book.Genre,
                IsAvailable = book.IsAvailable,
                CreatedAt = book.CreatedAt,
                CreatedBy = book.CreatedBy,
                UpdatedAt = book.UpdatedAt,
                UpdatedBy = book.UpdatedBy
            };

            return ApiResponse<BookDto>.Success(bookDto);
        }

        // get all active books
        public async Task<ApiResponse<List<BookDto>>> GetBooksAsync()
        {
            var books = await ActiveBooks
                .AsNoTracking()
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Author = b.Author,
                    Title = b.Title,
                    Genre = b.Genre,
                    IsAvailable = b.IsAvailable,
                    CreatedAt = b.CreatedAt,
                    CreatedBy = b.CreatedBy,
                    UpdatedAt = b.UpdatedAt,
                    UpdatedBy = b.UpdatedBy
                })
                .ToListAsync();

            return ApiResponse<List<BookDto>>.Success(books);
        }

        public async Task<ApiResponse<List<BookDto>>> GetAvailableBooksAsync()
        {
            var books = await AvailableBooks
                .AsNoTracking()
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Author = b.Author,
                    Title = b.Title,
                    Genre = b.Genre,
                    IsAvailable = b.IsAvailable,
                    CreatedAt = b.CreatedAt,
                    CreatedBy = b.CreatedBy,
                    UpdatedAt = b.UpdatedAt,
                    UpdatedBy = b.UpdatedBy
                })
                .ToListAsync();

            return ApiResponse<List<BookDto>>.Success(books);
        }

        public async Task<ApiResponse<List<BookDto>>> SearchBooksAsync(SearchBookDto search)
        {
            var query = ActiveBooks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search.Author))
                query = query.Where(b => EF.Functions.Like(b.Author, $"%{search.Author}%"));

            if (!string.IsNullOrWhiteSpace(search.Title))
                query = query.Where(b => EF.Functions.Like(b.Title, $"%{search.Title}%"));

            if (!string.IsNullOrWhiteSpace(search.Genre))
                query = query.Where(b => EF.Functions.Like(b.Genre, $"%{search.Genre}%"));


            var books = await query
                .AsNoTracking()
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Author = b.Author,
                    Title = b.Title,
                    Genre = b.Genre,
                    IsAvailable = b.IsAvailable,
                    CreatedAt = b.CreatedAt,
                    CreatedBy = b.CreatedBy,
                    UpdatedAt = b.UpdatedAt,
                    UpdatedBy = b.UpdatedBy
                })
                .ToListAsync();

            return ApiResponse<List<BookDto>>.Success(books);
        }

        public async Task<ApiResponse<BookDto>> CreateBookAsync(CreateBookDto dto, string user)
        {
            var book = new TblBook
            {
                Id = Guid.NewGuid().ToString(),
                Author = dto.Author,
                Title = dto.Title,
                Genre = dto.Genre,
                IsAvailable = true,
                DeleteFlag = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user
            };

            _db.TblBooks.Add(book);

            await _db.SaveChangesAsync();

            var bookDto = new BookDto
            {
                Id = book.Id,
                Author = book.Author,
                Title = book.Title,
                Genre = book.Genre,
                IsAvailable = book.IsAvailable,
                CreatedAt = book.CreatedAt,
                CreatedBy = book.CreatedBy,
                UpdatedAt = book.UpdatedAt,
                UpdatedBy = book.UpdatedBy
            };

            return ApiResponse<BookDto>.Success(bookDto, "Book created successfully.");
        }

        public async Task<ApiResponse<List<BookDto>>> BulkCreateBooksAsync(List<CreateBookDto> dtos, string user)
        {
            var books = dtos.Select(dto => new TblBook
            {
                Id = Guid.NewGuid().ToString(),
                Author = dto.Author,
                Title = dto.Title,
                Genre = dto.Genre,
                IsAvailable = true,
                DeleteFlag = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user
            }).ToList();

            _db.TblBooks.AddRange(books);
            await _db.SaveChangesAsync();

            var result = books.Select(b => new BookDto
            {
                Id = b.Id,
                Author = b.Author,
                Title = b.Title,
                Genre = b.Genre,
                IsAvailable = b.IsAvailable,
                CreatedAt = b.CreatedAt,
                CreatedBy = b.CreatedBy,
                UpdatedAt = b.UpdatedAt,
                UpdatedBy = b.UpdatedBy
            }).ToList();

            return ApiResponse<List<BookDto>>.Success(result, "Books bulk created successfully.");
        }

        public async Task<ApiResponse<bool>> UpdateBookAsync(string id, UpdateBookDto dto, string user)
        {
            var book = await ActiveBooks.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return ApiResponse<bool>.Failure("Book not found.");

            if (!string.IsNullOrEmpty(dto.Author)) book.Author = dto.Author;
            if (!string.IsNullOrEmpty(dto.Title)) book.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Genre)) book.Genre = dto.Genre;

            book.UpdatedAt = DateTime.UtcNow;
            book.UpdatedBy = user;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Book updated successfully.");
        }

        public async Task<ApiResponse<bool>> DeleteBookAsync(string id, string user)
        {
            var book = await ActiveBooks.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return ApiResponse<bool>.Failure("Book not found.");

            book.DeleteFlag = true;
            book.IsAvailable = false;
            book.UpdatedAt = DateTime.UtcNow;
            book.UpdatedBy = user;

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Book deleted successfully.");
        }

        // admin restore deleted books
        public async Task<ApiResponse<bool>> RestoreBookAsync(string id, string user)
        {
            var book = await _db.TblBooks.FirstOrDefaultAsync(b => b.Id == id && b.DeleteFlag);
            if (book == null) return ApiResponse<bool>.Failure("Deleted book not found.");

            book.DeleteFlag = false;
            book.IsAvailable = true;
            book.UpdatedAt = DateTime.UtcNow;
            book.UpdatedBy = user;

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Book restored successfully.");
        }

        // update book's avaialability status
        public async Task<ApiResponse<bool>> UpdateStatusAsync(string id, bool isAvailable, string user)
        {
            var book = await ActiveBooks.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return ApiResponse<bool>.Failure("Book not found.");

            book.IsAvailable = isAvailable;
            book.UpdatedAt = DateTime.UtcNow;
            book.UpdatedBy = user;

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Success(true, "Book status updated successfully.");
        }
    }
}
