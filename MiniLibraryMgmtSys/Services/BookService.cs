using Microsoft.EntityFrameworkCore;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTO;

namespace MiniLibraryMgmtSys.Services
{
    public sealed class BookService : IBookService
    {
        private readonly AppDbContext _db;

        public BookService(AppDbContext db)
        {
            _db = db;
        }

        private IQueryable<TblBook> ExistingBooks =>
        _db.TblBooks.Where(b => !b.DeleteFlag);

        private IQueryable<TblBook> AvailableBooks =>
            ExistingBooks.Where(b => b.IsAvailable);

        public async Task<List<BookDto>> GetAllBooksAsync()
        {
            return await _db.TblBooks
                .AsNoTracking()
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Author = b.Author,
                    Title = b.Title,
                    Genre = b.Genre,
                    IsAvailable = b.IsAvailable,
                    DeleteFlag = b.DeleteFlag
                })
                .ToListAsync();
        }

        public async Task<BookDto?> GetBookByIdAsync(string id)
        {
            var book = await ExistingBooks.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return null;

            return new BookDto
            {
                Id = book.Id,
                Author = book.Author,
                Title = book.Title,
                Genre = book.Genre,
                IsAvailable = book.IsAvailable
            };
        }

        public async Task<List<BookDto>> GetAvailableBooksAsync()
        {
            return await AvailableBooks
                .AsNoTracking()
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Author = b.Author,
                    Title = b.Title,
                    Genre = b.Genre,
                    IsAvailable = b.IsAvailable
                })
                .ToListAsync();
        }

        public async Task<List<BookDto>> SearchBooksAsync(SearchBookDto search)
        {
            var query = ExistingBooks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search.Author))
                query = query.Where(b => b.Author.Contains(search.Author));

            if (!string.IsNullOrWhiteSpace(search.Title))
                query = query.Where(b => b.Title.Contains(search.Title));

            if (!string.IsNullOrWhiteSpace(search.Genre))
                query = query.Where(b => b.Genre.Contains(search.Genre));

            return await query
                .AsNoTracking()
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Author = b.Author,
                    Title = b.Title,
                    Genre = b.Genre,
                    IsAvailable = b.IsAvailable
                })
                .ToListAsync();
        }

        public async Task<BookDto?> CreateBookAsync(CreateBookDto dto)
        {
            var book = new TblBook
            {
                Id = Guid.NewGuid().ToString(),
                Author = dto.Author,
                Title = dto.Title,
                Genre = dto.Genre,
                IsAvailable = true,
                DeleteFlag = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.TblBooks.Add(book);
            await _db.SaveChangesAsync();

            return new BookDto
            {
                Id = book.Id,
                Author = book.Author,
                Title = book.Title,
                Genre = book.Genre,
                IsAvailable = book.IsAvailable
            };
        }

        public async Task<bool> UpdateBookAsync(string id, UpdateBookDto dto)
        {
            var book = await ExistingBooks.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return false;

            if (!string.IsNullOrEmpty(dto.Author)) book.Author = dto.Author;
            if (!string.IsNullOrEmpty(dto.Title)) book.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Genre)) book.Genre = dto.Genre;

            book.UpdatedAt = DateTime.UtcNow;
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteBookAsync(string id)
        {
            var book = await ExistingBooks.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return false;

            book.DeleteFlag = true;
            book.IsAvailable = false;
            book.UpdatedAt = DateTime.UtcNow;

            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> RestoreBookAsync(string id)
        {
            var book = await _db.TblBooks.FirstOrDefaultAsync(b => b.Id == id && b.DeleteFlag);
            if (book == null) return false;

            book.DeleteFlag = false;
            book.IsAvailable = true;
            book.UpdatedAt = DateTime.UtcNow;

            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateBookStatusAsync(string id, bool isAvailable)
        {
            var book = await ExistingBooks.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return false;

            book.IsAvailable = isAvailable;
            book.UpdatedAt = DateTime.UtcNow;

            return await _db.SaveChangesAsync() > 0;
        }
    }   
}
