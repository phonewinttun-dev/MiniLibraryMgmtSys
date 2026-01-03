using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        private IQueryable<TblBook> AvailableBookQuery =>
            _db.TblBooks.AsNoTracking()
            .Where(book => book.DeleteFlag == false && book.IsAvailable == true);
        private IQueryable<TblBook> ExistingBookQuery => 
                    _db.TblBooks.AsNoTracking()
                    .Where(book => book.DeleteFlag == false);

        // GET: allBooks
        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            var allBookLst = await _db.TblBooks
                .Select(book => new BookDto
                {
                    Id = book.Id,
                    Author = book.Author,
                    Title = book.Title,
                    Genre = book.Genre,
                    IsAvailable = book.IsAvailable
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<BookDto>>
            {
                IsSuccess = true,
                Message = "Books retrieved successfully.",
                Data = allBookLst
            });
        }

        // GET: booksById/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooksById(string id)
        {
            // ID validation
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiResponse<BookDto>
                {
                    IsSuccess = false,
                    Message = "Invalid book ID."
                });
            }

            var book = await ExistingBookQuery
                .FirstOrDefaultAsync(b => b.Id == id);

            //check if book exists
            if (book is null)
            {
                return NotFound(new ApiResponse<BookDto>
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

            //return Ok(new BookResponseDto
            //{
            //    IsSuccess = true,
            //    Message = "Book retrieved successfully.",
            //    Data = result
            //});

            return Ok(new ApiResponse<BookDto>
            {
                IsSuccess = true,
                Message = "Book retrieved successfully.",
                Data = result
            });
        }

        // GET: booksAvailable
        [HttpGet]
        public async Task<IActionResult> GetAvailableBooks()
        {
            var availableBooks = await AvailableBookQuery
                .Select(book => new BookDto
                {
                    Id = book.Id,
                    Author = book.Author,
                    Title = book.Title,
                    Genre = book.Genre,
                    IsAvailable = book.IsAvailable
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<BookDto>>
            {
                IsSuccess = true,
                Message = "Books retrieved successfully.",
                Data = availableBooks
            });
        }

        // POST: createBook
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto request)
        {
            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Author))
            {
                return BadRequest(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "Author and Title are required."
                });
            }

            var newBook = new TblBook
            {
                Id = Guid.NewGuid().ToString(),
                Author = request.Author,
                Title = request.Title,
                Genre = request.Genre,
                IsAvailable = true,
                DeleteFlag = false,
                CreatedAt = DateTime.UtcNow
            };

            try
            {

                _db.TblBooks.Add(newBook);
                var createdBookResult = await _db.SaveChangesAsync();

                var createdBook = new BookDto
                {
                    Id = newBook.Id.ToString(),
                    Author = newBook.Author,
                    Title = newBook.Title,
                    Genre = newBook.Genre,
                    IsAvailable = newBook.IsAvailable
                };

                return CreatedAtAction(
                            nameof(GetBooksById),
                            new { id = newBook.Id },
                            new ApiResponse<BookDto>
                            {
                                IsSuccess = createdBookResult > 0,
                                Message = createdBookResult > 0 ? "Book created successfully!" : "Failed to create book.",
                                Data = createdBook  
                            }
                        );
            }
            catch
            {
                return StatusCode(500, new ApiResponse<CreateBookDto>
                {
                    IsSuccess = false,
                    Message = "An error occurred while creating the book."
                });
            }
        }

        // PATCH: partial update books/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateBook(string id, [FromBody] UpdateBookDto request)
        {
            var book = await _db.TblBooks
                .Where(b => !b.DeleteFlag)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book is null)
            {
                return NotFound(new ApiResponse<UpdateBookDto>
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

            book.UpdatedAt = DateTime.UtcNow;

            var patchedBookResult = await _db.SaveChangesAsync() > 0;

            return Ok(new ApiResponse<BookDto>
            {
                IsSuccess = patchedBookResult,
                Message = patchedBookResult ? "Book updated successfully!" : "Failed to update book.",
                //Data = new BookDto
                //{
                //    Id = book.Id,
                //    Author = book.Author,
                //    Title = book.Title,
                //    Genre = book.Genre,
                //    IsAvailable = book.IsAvailable
                //}
            });
        }

        // DELETE: deleteBook/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiResponse<Object>
                {
                    IsSuccess = false,
                    Message = "Book id is required."
                });
            }

            var book = await _db.TblBooks
                .FirstOrDefaultAsync(book => book.Id == id && !book.DeleteFlag);

            if (book is null)
            {
                return NotFound(new ApiResponse<Object>
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }

            //soft delete action
            book.IsAvailable = false;
            book.DeleteFlag = true;
            book.UpdatedAt = DateTime.UtcNow;

            var result = await _db.SaveChangesAsync() > 0;

            return Ok(new ApiResponse<Object>
            {
                IsSuccess = result,
                Message = result ? "Book deleted successfully!" : "Failed to delete book."
            });
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

            var query = AvailableBookQuery;

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
            var results = await AvailableBookQuery
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
