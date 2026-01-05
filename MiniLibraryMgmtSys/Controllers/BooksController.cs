//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.HttpResults;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTO;
using System.Data;
//using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace MiniLibraryMgmtSys.Controllers
{
    [Route("api/books")]
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
        [HttpGet("allBooks")]
        public async Task<IActionResult> GetAllBooks()
        {
            var allBookLst = await _db.TblBooks
                .Select(book => new BookDto
                {
                    Id = book.Id,
                    Author = book.Author,
                    Title = book.Title,
                    Genre = book.Genre,
                    IsAvailable = book.IsAvailable,
                    DeleteFlag = book.DeleteFlag,
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
        [HttpGet("booksById/{id}")]
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
        [HttpGet("booksAvailable")]
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

        // GET: searchByFilter
        [HttpGet("booksSearch")]
        public async Task<IActionResult> SearchByFilter(SearchBookDto searchRequest)
        {
            if (string.IsNullOrWhiteSpace(searchRequest.Author) &&
                string.IsNullOrWhiteSpace(searchRequest.Title) &&
                string.IsNullOrWhiteSpace(searchRequest.Genre))
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "At least one search filter must be provided."
                });
            }

            var query = ExistingBookQuery;

            if (!string.IsNullOrWhiteSpace(searchRequest.Author))
                query = query.Where(b => b.Author.Contains(searchRequest.Author.Trim()));

            if (!string.IsNullOrEmpty(searchRequest.Title))
                query = query.Where(b => b.Title.Contains(searchRequest.Title.Trim()));

            if (!string.IsNullOrEmpty(searchRequest.Genre))
                query = query.Where(b => b.Genre.Contains(searchRequest.Genre.Trim()));

            var results = await query
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = b.Genre,
                    IsAvailable = b.IsAvailable
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<BookDto>>
            {
                IsSuccess = true,
                Message = "Search completed.",
                Data = results
            });
        }

        // POST: createBook
        [HttpPost("booksCreate")]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto request)
        {
            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Author))
            {
                return BadRequest(new ApiResponse<CreateBookDto>
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
        [HttpPatch("booksUpdate/{id}")]
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

            try
            {
                var patchedBookResult = await _db.SaveChangesAsync() > 0;

                return Ok(new ApiResponse<BookDto>
                {
                    IsSuccess = true,
                    Message = "Book updated successfully!"
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
            catch
            {
                return StatusCode(500, new ApiResponse<UpdateBookDto>
                {
                    IsSuccess = false,
                    Message = "An error occurred while updating the book."
                });
            }
        }

        // DELETE: deleteBook/{id}
        [HttpDelete("booksDelete/{id}")]
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

        [HttpPatch("booksRestore/{id}")]
        public async Task<IActionResult> RestoreBooks(string id)
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
                .FirstOrDefaultAsync(book => book.Id == id && book.DeleteFlag);

            if (book is null)
            {
                return NotFound(new ApiResponse<BookDto>
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }

            book.DeleteFlag = false;
            book.IsAvailable = true;
            book.UpdatedAt = DateTime.UtcNow;

            var result = await _db.SaveChangesAsync() > 0;

            try
            {
                var updatedStatus = new BookDto
                {
                    DeleteFlag = book.DeleteFlag,
                    IsAvailable = book.IsAvailable
                };

                return Ok(new ApiResponse<BookDto>
                {
                    IsSuccess = result,
                    Message = result ? "Book restored successfully!" : "Failed to restore book."
                });
            }
            catch
            {
                return StatusCode(500, new ApiResponse<BookDto>
                {
                    IsSuccess = false,
                    Message = "An error occurred while restoring the book."
                });
            }

        }

        // for borrowing and returning books
        // PATCH: setBookStatus/{id}
        [HttpPatch("booksStatus/{id}")]
        public async Task<IActionResult> SetBookStatus(string id, [FromBody] UpdateBookAvailabilityDto request)
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
                return NotFound(new ApiResponse<BookDto>
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }

            book.IsAvailable = request.IsAvailable;
            book.UpdatedAt = DateTime.UtcNow;

            var result = await _db.SaveChangesAsync() > 0;

            try
            {
                var updatedStatus = new BookDto
                {
                    IsAvailable = request.IsAvailable
                };
                
                return Ok(new ApiResponse<BookDto>
                {
                    IsSuccess = result,
                    Message = result ? "Book status updated successfully!" : "Failed to update status"
                });
            }
            catch
            {
                return StatusCode(500, new ApiResponse<BookDto>
                {
                    IsSuccess = false,
                    Message = "An error occurred while updating the book status."
                });
            }
            
        }

        

    }

}
