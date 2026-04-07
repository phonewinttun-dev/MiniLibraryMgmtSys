using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.DTO;
using MiniLibraryMgmtSys.Infrastructure;
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

            return Ok(ApiResponse<List<BookDto>>.Success(books, "Books retrieved successfully."));
        }

        // GET: api/availableBooks
        [HttpGet("availableBooks")]
        [Authorize(Roles = "Admin, Librarian, Member")]
        public async Task<IActionResult> GetAvaialable()
        {
            var books = await _bookService.GetAvailableBooksAsync();

            return Ok(ApiResponse<List<BookDto>>.Success(books, "Books retrieved successfully."));
        }

        // GET: api/books/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Librarian, Member")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(ApiResponse<BookDto>.Failure("Invalid book ID."));
            }

            var book = await _bookService.GetBookByIdAsync(id);

            if (book is null)
            {
                return NotFound(ApiResponse<BookDto>.Failure("Book not found."));
            }

            return Ok(ApiResponse<BookDto>.Success(book, "Book retrieved successfully."));
        }

        // GET: api/books/search
        [HttpGet("search")]
        [Authorize(Roles = "Admin, Librarian, Member")]
        public async Task<IActionResult> GetBySearch([FromQuery] SearchBookDto search)
        {
            var books = await _bookService.SearchBooksAsync(search);

            return Ok(ApiResponse<List<BookDto>>.Success(books, "Books retrieved successfully."));
        }

        // POST: api/books
        [HttpPost]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> Create([FromBody] CreateBookDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failure("Invalid book data."));

            try
            {
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";

                var createdBook = await _bookService.CreateBookAsync(request, user);

                if (createdBook == null)
                {
                    return StatusCode(500, ApiResponse<BookDto>.Failure("Failed to create book."));
                }

                _logger.LogInformation(
                    "Book created successfully. BookId: {BookId}, User: {User}",
                    createdBook.Id,
                    user
                );

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = createdBook!.Id },
                    ApiResponse<BookDto>.Success(createdBook, "Book created successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a book.");

                return StatusCode(500, ApiResponse<BookDto>.Failure("An unexpected error occured."));
            }

        }

        // POST: api/books/bulkInsert
        [HttpPost("bulkInsert")]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> BulkCreate([FromBody] List<CreateBookDto> requests)
        {
            if (requests == null || !requests.Any())
                return BadRequest(ApiResponse<object>.Failure("Book list cannot be empty."));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failure("Invalid book data in list."));

            try
            {
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var createdBooks = await _bookService.BulkCreateBooksAsync(requests, user);

                _logger.LogInformation("{Count} books created successfully.", createdBooks.Count);

                return Ok(ApiResponse<List<BookDto>>.Success(createdBooks, $"{createdBooks.Count} books created successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while bulk creating books.");

                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred during bulk creation."));
            }
        }

        // PATCH: api/{id}
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateBookDto request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Failure("Invalid book data."));
            }

            try
            {
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var updated = await _bookService.UpdateBookAsync(id, request, user);

                if (!updated)
                {
                    return NotFound(ApiResponse<object>.Failure("Book not found."));
                }

                _logger.LogInformation("Book updated successfully with ID: {BookId}", id);

                return Ok(ApiResponse<object>.Success(true, "Book updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating the book with ID: {BookId}", id);

                return StatusCode(500, ApiResponse<object>.Failure($"An error occurred while updating the book: {ex.Message}"));
            }

        }

        // PATCH: api/books/{id}/status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> UpdateStatus(string id, [FromQuery] bool isAvailable)
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
            var result = await _bookService.UpdateStatusAsync(id, isAvailable, user);

            if (!result)
                return NotFound(ApiResponse<object>.Failure("Book not found."));

            return Ok(ApiResponse<object>.Success(true, "Book status updated successfully."));
        }

        // DELETE: api/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(ApiResponse<object>.Failure("Book id is required."));
            }

            try
            {
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var deleted = await _bookService.DeleteBookAsync(id, user);

                if (!deleted)
                {
                    return NotFound(ApiResponse<object>.Failure("Book not found."));
                }

                _logger.LogInformation("Book deleted successfully with ID: {BookId}", id);

                return Ok(ApiResponse<object>.Success(true, "Book deleted successfully."));
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error occurred while deleting the book with ID: {BookId}", id);

                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred while deleting the book."));
            }

        }
    }




}


