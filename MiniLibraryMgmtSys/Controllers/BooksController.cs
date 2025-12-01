using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTO;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MiniLibraryMgmtSys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {

        private readonly AppDbContext _db;
        public BooksController(AppDbContext db)
        {
            _db = db;
        }


        //Book Table Query
        private IQueryable<TblBook> BookQuery => _db.TblBooks.AsNoTracking().Where(book => book.DeleteFlag == false);

        //private IQueryable<TblBook> AvailableBooks => _db.TblBooks.Where(book => book.IsAvailable == true && book.DeleteFlag == false).AsNoTracking();

        // To check if the Book table is empty
        //[HttpGet("getAllBooks")]
        //public IActionResult GetAllBooks()
        //{
        //    var lst = BookQuery
        //                .Select(book => new BookDto
        //                {
        //                    Id = book.Id,
        //                    Author = book.Author,
        //                    Title = book.Title,
        //                    Genre = book.Genre,
        //                    IsAvailable = book.IsAvailable
        //                })
        //                .ToList();

        //    return Ok(lst);
        //}

        //// For users to search books by ID
        //[HttpGet("getBooksById/{id}")]
        //public IActionResult GetBooksById(string id)
        //{
        //    var book = BookQuery.FirstOrDefault(book => book.Id == id);

        //    if (book is null)
        //    {
        //        return NotFound(new BookResponseDto
        //        {
        //            IsSuccess = false,
        //            Message = "Book not found."
        //        });
        //    }
        //    var result = new BookDto
        //    {
        //        Id = book.Id,
        //        Author = book.Author,
        //        Title = book.Title,
        //        Genre = book.Genre,
        //        IsAvailable = book.IsAvailable
        //    };

        //    return Ok(new BookResponseDto
        //    {
        //        IsSuccess = true,
        //        Message = "Book retrieved successfully.",
        //        Data = result
        //    });
        //}

        //[HttpPost("createBook")]
        //public IActionResult CreateBook([FromBody] BookDto request)
        //{
        //    if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Author))
        //        return BadRequest(new BookResponseDto
        //        {
        //            IsSuccess = false,
        //            Message = "Author and Title are required."
        //        });

        //    try
        //    {
        //        var newBook = new TblBook
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            Author = request.Author,
        //            Title = request.Title,
        //            Genre = request.Genre,
        //            IsAvailable = true,
        //            DeleteFlag = false
        //        };

        //        _db.TblBooks.Add(newBook);

        //        request.CreatedAt = DateTime.Now;

        //        var result = _db.SaveChanges();

        //        string message = result > 0 ? "Book created successfully." : "Failed to create book.";

        //        BookResponseDto response = new BookResponseDto
        //        {
        //            IsSuccess = result > 0,
        //            Message = message,
        //        };

        //        return Ok(response);
        //    }

        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new BookResponseDto
        //        {
        //            IsSuccess = false,
        //            Message = "An error occurred while creating the book."
        //        });
        //    }

        //}

        //[HttpPatch("updateBook/{id}")]
        //public IActionResult UpdateBook(string id, [FromBody] BookDto request)
        //{
        //    var book = _db.TblBooks.FirstOrDefault(book => book.Id == id);

        //    if (book is null)
        //    {
        //        return NotFound(new BookResponseDto
        //        {
        //            IsSuccess = false,
        //            Message = "Book not found."
        //        });
        //    }

        //    if (!string.IsNullOrEmpty(request.Author))
        //    {
        //        book.Author = request.Author;
        //    }

        //    if (!string.IsNullOrEmpty(request.Title))
        //    {
        //        book.Title = request.Title;
        //    }

        //    if (!string.IsNullOrEmpty(request.Genre))
        //    {
        //        book.Genre = request.Genre;
        //    }

        //    book.UpdatedAt = DateTime.Now;

        //    var result = _db.SaveChanges() > 0;

        //    return Ok(new BookResponseDto
        //    {
        //        IsSuccess = result,
        //        Message = result ? "Book updated successfully!" : "Failed to update book."
        //    });

        //}

        //[HttpDelete("deleteBook/{id}")]
        //public IActionResult DeleteBook(string id)
        //{
        //    var book = _db.TblBooks
        //        .FirstOrDefault(book => book.Id == id && book.DeleteFlag == false);

        //    if (book is null)
        //    {
        //        return NotFound(new BookResponseDto
        //        {
        //            IsSuccess = false,
        //            Message = "Book not found."
        //        });
        //    }

        //    book.IsAvailable = false;
        //    book.DeleteFlag = true;
        //    book.UpdatedAt = DateTime.Now;

        //    var result = _db.SaveChanges() > 0;

        //    return Ok(new BookResponseDto
        //    {
        //        IsSuccess = result,
        //        Message = result ? "Book deleted successfully!" : "Failed to delete book."
        //    });
        //}

        //[HttpGet("getAvailableBooks")]
        //public IActionResult GetAvailableBooks()
        //{
        //    var availableBooks = BookQuery
        //                .Where(book => book.IsAvailable)
        //                .Select(book => new
        //                {
        //                    book.Id,
        //                    book.Author,
        //                    book.Title,
        //                    book.Genre,
        //                    book.IsAvailable
        //                })
        //                .ToList();

        //    return Ok(availableBooks);

        //}

        ////[HttpPatch("setUnavailable/{id}")]
        ////public IActionResult SetUnavailable(string id)
        ////{
        ////    var book = BookQuery.FirstOrDefault(book => book.Id == id);

        ////    if (book is null)
        ////    {
        ////        return NotFound(new BookResponseDto
        ////        {
        ////            IsSuccess = false,
        ////            Message = "Book not found."
        ////        });
        ////    }

        ////    book.IsAvailable = false;
        ////    book.UpdatedAt = DateTime.Now;

        ////    return Ok(new BookResponseDto
        ////    {
        ////        IsSuccess = _db.SaveChanges() > 0,
        ////        Message = "Book status updated successfully!"
        ////    });

        ////}

        //[HttpPatch("setBookStatus/{id}")]
        //public IActionResult SetBookStatus(string id, [FromBody] BookDto request)
        //{
        //    var book = _db.TblBooks.FirstOrDefault(book => book.Id == id && book.DeleteFlag == false);

        //    if (book is null)
        //    {
        //        return NotFound(new BookResponseDto
        //        {
        //            IsSuccess = false,
        //            Message = "Book not found."
        //        });
        //    }

        //    book.IsAvailable = request.IsAvailable;
        //    book.UpdatedAt = DateTime.Now;

        //    var result = _db.SaveChanges() > 0;

        //    return Ok(new BookResponseDto
        //    {
        //        IsSuccess = result,
        //        Message = result ? "Book status updated successfully!" : "Failed to update book status."
        //    });

        //}

        ////[HttpGet("searchBooks")]
        ////public IActionResult SearchBooks([FromQuery] string query)
        ////{
        ////    var results = BookQuery
        ////                    .Where(book => book.Title.Contains(query) || book.Author.Contains(query) || book.Genre.Contains(query))
        ////                    .Select(book => new
        ////                    {
        ////                        book.Id,
        ////                        book.Author,
        ////                        book.Title,
        ////                        book.Genre,
        ////                        book.IsAvailable
        ////                    })
        ////                    .ToList();
        ////    return Ok(results);
        ////}

        //[HttpGet("searchByFilter")]
        //public IActionResult SearchByFilter(string? author, string? title, string? genre)
        //{

        //    if (author == null && title == null && genre == null)
        //    {
        //        return BadRequest("You must enter one of the fields to search.");
        //    }

        //    var query = BookQuery;

        //    if (!string.IsNullOrEmpty(author))
        //        query = query.Where(b => b.Author.Contains(author));

        //    if (!string.IsNullOrEmpty(title))
        //        query = query.Where(b => b.Title.Contains(title));


        //    if (!string.IsNullOrEmpty(genre))
        //        query = query.Where(b => b.Genre.Contains(genre));

        //    var results = query.ToList();

        //    if (!results.Any())
        //    {
        //        return NotFound(new BookResponseDto
        //        {
        //            IsSuccess = false,
        //            Message = "No books found matching the given criteria."
        //        });
        //    }

        //    return Ok(results);
        //}

        //[HttpGet("getBooksByGenre")]
        //public IActionResult GetBooksByGenre(string genre)
        //{
        //    var results = BookQuery
        //                    .Where(book => book.Genre != null && book.Genre.ToLower() == genre.ToLower())
        //                    .Select(book => new
        //                    {
        //                        book.Author,
        //                        book.Title,
        //                        book.Genre,
        //                        book.IsAvailable
        //                    })
        //                    .ToList();

        //    if (!results.Any())
        //    {
        //        return NotFound(new BookResponseDto
        //        {
        //            IsSuccess = false,
        //            Message = "No books found in the specified genre."
        //        });
        //    }

        //    return Ok(results);
        //}

        // GET: getAllBooks
        [HttpGet("getAllBooks")]
        public async Task<IActionResult> GetAllBooks()
        {
            var lst = await BookQuery
                .Select(book => new BookDto
                {
                    Id = book.Id,
                    Author = book.Author,
                    Title = book.Title,
                    Genre = book.Genre,
                    IsAvailable = book.IsAvailable
                })
                .ToListAsync();

            return Ok(lst);
        }

        // GET: getBooksById/{id}
        [HttpGet("getBooksById/{id}")]
        public async Task<IActionResult> GetBooksById(string id)
        {
            var book = await BookQuery.FirstOrDefaultAsync(book => book.Id == id);

            if (book is null)
            {
                return NotFound(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }

            var result = new BookDto
            {
                Id = book.Id,
                Author = book.Author,
                Title = book.Title,
                Genre = book.Genre,
                IsAvailable = book.IsAvailable
            };

            return Ok(new BookResponseDto
            {
                IsSuccess = true,
                Message = "Book retrieved successfully.",
                Data = result
            });
        }

        // POST: createBook
        [HttpPost("createBook")]
        public async Task<IActionResult> CreateBook([FromBody] BookDto request)
        {
            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Author))
            {
                return BadRequest(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "Author and Title are required."
                });
            }

            try
            {
                var newBook = new TblBook
                {
                    Id = Guid.NewGuid().ToString(),
                    Author = request.Author,
                    Title = request.Title,
                    Genre = request.Genre,
                    IsAvailable = true,
                    DeleteFlag = false,
                    CreatedAt = DateTime.Now
                };

                await _db.TblBooks.AddAsync(newBook);
                var result = await _db.SaveChangesAsync();

                return Ok(new BookResponseDto
                {
                    IsSuccess = result > 0,
                    Message = result > 0 ? "Book created successfully." : "Failed to create book."
                });
            }
            catch
            {
                return StatusCode(500, new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "An error occurred while creating the book."
                });
            }
        }

        // PATCH: updateBook/{id}
        [HttpPatch("updateBook/{id}")]
        public async Task<IActionResult> UpdateBook(string id, [FromBody] BookDto request)
        {
            var book = await _db.TblBooks.FirstOrDefaultAsync(b => b.Id == id);

            if (book is null)
            {
                return NotFound(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }

            if (!string.IsNullOrEmpty(request.Author))
                book.Author = request.Author;

            if (!string.IsNullOrEmpty(request.Title))
                book.Title = request.Title;

            if (!string.IsNullOrEmpty(request.Genre))
                book.Genre = request.Genre;

            book.UpdatedAt = DateTime.Now;

            var result = await _db.SaveChangesAsync() > 0;

            return Ok(new BookResponseDto
            {
                IsSuccess = result,
                Message = result ? "Book updated successfully!" : "Failed to update book."
            });
        }

        // DELETE: deleteBook/{id}
        [HttpDelete("deleteBook/{id}")]
        public async Task<IActionResult> DeleteBook(string id)
        {
            var book = await _db.TblBooks
                .FirstOrDefaultAsync(book => book.Id == id && !book.DeleteFlag);

            if (book is null)
            {
                return NotFound(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }

            book.IsAvailable = false;
            book.DeleteFlag = true;
            book.UpdatedAt = DateTime.Now;

            var result = await _db.SaveChangesAsync() > 0;

            return Ok(new BookResponseDto
            {
                IsSuccess = result,
                Message = result ? "Book deleted successfully!" : "Failed to delete book."
            });
        }

        // GET: getAvailableBooks
        [HttpGet("getAvailableBooks")]
        public async Task<IActionResult> GetAvailableBooks()
        {
            var availableBooks = await BookQuery
                .Where(book => book.IsAvailable)
                .Select(book => new
                {
                    book.Id,
                    book.Author,
                    book.Title,
                    book.Genre,
                    book.IsAvailable
                })
                .ToListAsync();

            return Ok(availableBooks);
        }

        // PATCH: setBookStatus/{id}
        [HttpPatch("setBookStatus/{id}")]
        public async Task<IActionResult> SetBookStatus(string id, [FromBody] BookDto request)
        {
            var book = await _db.TblBooks
                .FirstOrDefaultAsync(book => book.Id == id && !book.DeleteFlag);

            if (book is null)
            {
                return NotFound(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }

            book.IsAvailable = request.IsAvailable;
            book.UpdatedAt = DateTime.Now;

            var result = await _db.SaveChangesAsync() > 0;

            return Ok(new BookResponseDto
            {
                IsSuccess = result,
                Message = result ? "Book status updated successfully!" : "Failed to update book status."
            });
        }

        // GET: searchByFilter
        [HttpGet("searchByFilter")]
        public async Task<IActionResult> SearchByFilter(string? author, string? title, string? genre)
        {
            if (author == null && title == null && genre == null)
            {
                return BadRequest("You must enter one of the fields to search.");
            }

            var query = BookQuery;

            if (!string.IsNullOrEmpty(author))
                query = query.Where(b => b.Author.Contains(author));

            if (!string.IsNullOrEmpty(title))
                query = query.Where(b => b.Title.Contains(title));

            if (!string.IsNullOrEmpty(genre))
                query = query.Where(b => b.Genre.Contains(genre));

            var results = await query.ToListAsync();

            if (!results.Any())
            {
                return NotFound(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "No books found matching the given criteria."
                });
            }

            return Ok(results);
        }

        // GET: getBooksByGenre
        [HttpGet("getBooksByGenre")]
        public async Task<IActionResult> GetBooksByGenre(string genre)
        {
            var results = await BookQuery
                .Where(book => book.Genre != null && book.Genre.ToLower() == genre.ToLower())
                .Select(book => new
                {
                    book.Author,
                    book.Title,
                    book.Genre,
                    book.IsAvailable
                })
                .ToListAsync();

            if (!results.Any())
            {
                return NotFound(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "No books found in the specified genre."
                });
            }

            return Ok(results);
        }
    }

}
