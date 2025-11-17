using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTO;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static MiniLibraryMgmtSys.DTO.BookDTO;

namespace MiniLibraryMgmtSys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext db;
        public BooksController()
        {
            db = new AppDbContext();
        }

        //Book Table Query
        private IQueryable<TblBook> BookQuery =>
            db.TblBooks.Where(book => book.DeleteFlag == false);

        private IQueryable<TblBook> AvailableBooks =>
            db.TblBooks.Where(book => book.IsAvailable == true && book.DeleteFlag == false);

        // To check if the Book table is empty
        [HttpGet("getAllBooks")]
        public IActionResult GetBooks()
        {
            var lst = BookQuery
                        .Select(book => new
                        {
                            book.Id,
                            book.Author,
                            book.Title,
                            book.Genre,
                            book.IsAvailable
                        })
                        .ToList();

            return Ok(lst);
        }

        // For users to search books by ID
        [HttpGet("getBooksById/{id}")]
        public IActionResult GetBooksById(string id)
        {
            var book = BookQuery.FirstOrDefault(book => book.Id == id);

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
                IsAvailable = book.IsAvailable,
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt,
                CreatedBy = book.CreatedBy,
                UpdatedBy = book.UpdatedBy
            };

            return Ok(new BookResponseDto
            {
                IsSuccess = true,
                Message = "Book retrieved successfully.",
                Data = result
            });
        }

        [HttpPost("createBook")]
        public IActionResult CreateBook([FromBody] BookDto request)
        {
            if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Author))
                return BadRequest(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "Author and Title are required."
                });

            try
            {
                db.TblBooks.Add(new TblBook
                {
                    Id = Guid.NewGuid().ToString(),
                    Author = request.Author,
                    Title = request.Title,
                    Genre = request.Genre,
                    IsAvailable = true,
                    DeleteFlag = false
                });

                request.CreatedAt = DateTime.Now;

                var result = db.SaveChanges();

                string message = result > 0 ? "Book created successfully." : "Failed to create book.";

                BookResponseDto response = new BookResponseDto
                {
                    IsSuccess = result > 0,
                    Message = message,
                };

                return Ok(response);
            }

            catch (Exception ex)
            {
                return StatusCode(500, new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "An error occurred while creating the book."
                });
            }

        }

        [HttpPatch("updateBook/{id}")]
        public IActionResult UpdateBook(string id, [FromBody] BookDto request)
        {
            var book = BookQuery.FirstOrDefault(book => book.Id == id);

            if (book is null)
            {
                return NotFound(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }

            if (!string.IsNullOrEmpty(request.Author))
            {
                book.Author = request.Author;
            }

            if (!string.IsNullOrEmpty(request.Title))
            {
                book.Title = request.Title;
            }

            if (!string.IsNullOrEmpty(request.Genre))
            {
                book.Genre = request.Genre;
            }

            book.UpdatedAt = DateTime.Now;

            var result = db.SaveChanges() > 0;

            return Ok(new BookResponseDto
            {
                IsSuccess = result,
                Message = result ? "Book updated successfully!" : "Failed to update book."
            });

        }

        [HttpDelete("deleteBook/{id}")]
        public IActionResult DeleteBook(string id)
        {
            var book = BookQuery.FirstOrDefault(book => book.Id == id);

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

            return Ok(new BookResponseDto
            {
                IsSuccess = db.SaveChanges() > 0,
                Message = "Book deleted successfully!"
            });
        }

        [HttpGet("getAvailableBooks")]
        public IActionResult GetAvailableBooks()
        {
            var lst = AvailableBooks
                        .Select(book => new
                        {
                            book.Id,
                            book.Author,
                            book.Title,
                            book.Genre,
                            book.IsAvailable
                        })
                        .ToList();

            return Ok(lst);

        }

        [HttpPatch("setUnavailable/{id}")]
        public IActionResult SetUnavailable(string id)
        {
            var book = BookQuery.FirstOrDefault(book => book.Id == id);

            if (book is null)
            {
                return NotFound(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }

            book.IsAvailable = false;
            book.UpdatedAt = DateTime.Now;

            return Ok(new BookResponseDto
            {
                IsSuccess = db.SaveChanges() > 0,
                Message = "Book status updated successfully!"
            });

        }

        [HttpPatch("setBookStatus/{id}")]
        public IActionResult SetBookStatus(string id, [FromBody] BookDto request)
        {
            var book = BookQuery.FirstOrDefault(book => book.Id.Equals(id));

            if (book is null)
            {
                return NotFound(new BookResponseDto
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }

            book.IsAvailable = request.IsAvailable;
            book.UpdatedAt = request.UpdatedAt;

            return Ok(new BookResponseDto
            {
                IsSuccess = db.SaveChanges() > 0,
                Message = "Book status updated successfully!"
            });

        }

        //[HttpGet("searchBooks")]
        //public IActionResult SearchBooks([FromQuery] string query)
        //{
        //    var results = BookQuery
        //                    .Where(book => book.Title.Contains(query) || book.Author.Contains(query) || book.Genre.Contains(query))
        //                    .Select(book => new
        //                    {
        //                        book.Id,
        //                        book.Author,
        //                        book.Title,
        //                        book.Genre,
        //                        book.IsAvailable
        //                    })
        //                    .ToList();
        //    return Ok(results);
        //}

        [HttpGet("searchByFilter")]
        public IActionResult SearchByFilter(string? author, string? title, string? genre)
        {
            var query = BookQuery;

            if (author == null && title == null && genre == null)
            {
                return BadRequest("You must enter one of the fields to search.");
            }

            if (!string.IsNullOrEmpty(author))
                query = query.Where(b => b.Author.Contains(author));

            if (!string.IsNullOrEmpty(title))
                query = query.Where(b => b.Title.Contains(title));


            if (!string.IsNullOrEmpty(genre))
                query = query.Where(b => b.Genre.Contains(genre));

            var results = query.ToList();

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

        [HttpGet("getBooksByGenre")]
        public IActionResult GetBooksByGenre(string genre)
        {
            var results = BookQuery
                            .Where(book => book.Genre.ToLower() == genre.ToLower())
                            .Select(book => new
                            {
                                book.Author,
                                book.Title,
                                book.Genre,
                                book.IsAvailable
                            })
                            .ToList();

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
