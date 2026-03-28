using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.DTO;
using MiniLibraryMgmtSys.Services;
using System.Security.Claims;


namespace MiniLibraryMgmtSys.Controllers
{
    [ApiController]
    [Route("api/books")]
    [Authorize]
    public class BooksController : ControllerBase
    {

        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }


        // GET: api/books/
        //[HttpGet]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> GetAll()
        //{
        //    var books = await _bookService.GetBooksAsync();

        //    return Ok(new ApiResponse<List<BookDto>>
        //    {
        //        IsSuccess = true,
        //        Message = "Books retrieved successfully.",
        //        Data = books
        //    });
        //}

        // GET: api/existingBooks
        [HttpGet("existingBooks")]
        [Authorize(Roles = "Admin, Librarian, Member")]
        public async Task<IActionResult> Get()
        {
            var books = await _bookService.GetBooksAsync();

            return Ok(new ApiResponse<List<BookDto>>
            {
                IsSuccess = true,
                Message = "Books retrieved successfully.",
                Data = books
            });
        }

        // GET: api/availableBooks
        [HttpGet("availableBooks")]
        [Authorize(Roles = "Admin, Librarian, Member")]
        public async Task<IActionResult> GetAvaialable()
        {
            var books = await _bookService.GetAvailableBooksAsync();

            return Ok(new ApiResponse<List<BookDto>>
            {
                IsSuccess = true,
                Message = "Books retrieved successfully.",
                Data = books
            });
        }

        // GET: api/books/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Librarian, Member")]
        public async Task<IActionResult> GetById(string id)
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

        // POST: api/books
        [HttpPost]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> Create([FromBody] CreateBookDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invalid book data."
                });

            try
            {
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";

                var createdBook = await _bookService.CreateBookAsync(request, user);

                if (createdBook == null)
                {
                    return StatusCode(500, new ApiResponse<BookDto>
                    {
                        IsSuccess = false,
                        Message = "Failed to create book."
                    });
                }

                _logger.LogInformation(
                    "Book created successfully. BookId: {BookId}, User: {User}",
                    createdBook.Id,
                    user
                );

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdBook!.Id },
                    new ApiResponse<BookDto>
                    {
                        IsSuccess = true,
                        Message = "Book created successfully.",
                        Data = createdBook
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a book.");

                return StatusCode(500, new ApiResponse<BookDto>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occured."
                });
            }

        }

        // POST: api/books/bulkInsert
        [HttpPost("bulkInsert")]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> BulkCreate([FromBody] List<CreateBookDto> requests)
        {
            if (requests == null || !requests.Any())
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Book list cannot be empty."
                });

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invalid book data in list."
                });

            try
            {
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var createdBooks = await _bookService.BulkCreateBooksAsync(requests, user);

                _logger.LogInformation("{Count} books created successfully.", createdBooks.Count);

                return Ok(new ApiResponse<List<BookDto>>
                {
                    IsSuccess = true,
                    Message = $"{createdBooks.Count} books created successfully.",
                    Data = createdBooks
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while bulk creating books.");

                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred during bulk creation."
                });
            }
        }

        // PATCH: api/{id}
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateBookDto request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Invalid book data."
                });
            }

            try
            {
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var updated = await _bookService.UpdateBookAsync(id, request, user);

                if (!updated)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Book not found."
                    });
                }

                _logger.LogInformation("Book updated successfully with ID: {BookId}", id);

                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "Book updated successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating the book with ID: {BookId}", id);

                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"An error occurred while updating the book: {ex.Message}"
                });
            }

        }

        // DELETE: api/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "Book id is required."
                });
            }

            try
            {
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var deleted = await _bookService.DeleteBookAsync(id, user);

                if (!deleted)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Book not found."
                    });
                }

                _logger.LogInformation("Book deleted successfully with ID: {BookId}", id);

                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "Book deleted successfully."
                });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error occurred while deleting the book with ID: {BookId}", id);

                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "An unexpected error occurred while deleting the book."
                });
            }

        }
    }




}


