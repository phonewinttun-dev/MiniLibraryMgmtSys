//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Http.HttpResults;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTO;
using MiniLibraryMgmtSys.Services;
using System.Data;
//using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace MiniLibraryMgmtSys.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // GET: api/books/allBooks
        [HttpGet("allBooks")]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _bookService.GetAllBooksAsync();

            return Ok(new ApiResponse<List<BookDto>>
            {
                IsSuccess = true,
                Message = "Books retrieved successfully.",
                Data = books
            });
        }

        // GET: api/books/booksById/{id}
        [HttpGet("booksById/{id}")]
        public async Task<IActionResult> GetBookById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiResponse<BookDto>
                {
                    IsSuccess = false,
                    Message = "Invalid book ID."
                });
            }

            var book = await _bookService.GetBookByIdAsync(id);

            if (book is null)
            {
                return NotFound(new ApiResponse<BookDto>
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }

            return Ok(new ApiResponse<BookDto>
            {
                IsSuccess = true,
                Message = "Book retrieved successfully.",
                Data = book
            });
        }

        // POST: api/books/booksCreate
        [HttpPost("booksCreate")]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Title) ||
                string.IsNullOrWhiteSpace(request.Author))
            {
                return BadRequest(new ApiResponse<BookDto>
                {
                    IsSuccess = false,
                    Message = "Author and Title are required."
                });
            }

            var createdBook = await _bookService.CreateBookAsync(request);

            return CreatedAtAction(
                nameof(GetBookById),
                new { id = createdBook!.Id },
                new ApiResponse<BookDto>
                {
                    IsSuccess = true,
                    Message = "Book created successfully.",
                    Data = createdBook
                });
        }

        // PATCH: api/books/booksUpdate/{id}
        [HttpPatch("booksUpdate/{id}")]
        public async Task<IActionResult> UpdateBook(string id, [FromBody] UpdateBookDto request)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Book id is required."
                });
            }

            var updated = await _bookService.UpdateBookAsync(id, request);

            if (!updated)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Book updated successfully."
            });
        }

        // DELETE: api/books/booksDelete/{id}
        [HttpDelete("booksDelete/{id}")]
        public async Task<IActionResult> DeleteBook(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Book id is required."
                });
            }

            var deleted = await _bookService.DeleteBookAsync(id);

            if (!deleted)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Book not found."
                });
            }

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "Book deleted successfully."
            });
        }
    }




}


