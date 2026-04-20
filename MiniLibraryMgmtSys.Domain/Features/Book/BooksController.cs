using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.DTO;
using MiniLibraryMgmtSys.Infrastructure;
using System.Security.Claims;


namespace MiniLibraryMgmtSys.Domain.Features.Book
{
    [ApiController]
    [Route("api/books")]
    [Authorize]
    public class BooksController : ControllerBase
    {

        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
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

        [HttpGet("existingBooks")]
        [Authorize(Roles = "Admin, Librarian, Member")]
        public async Task<IActionResult> Get()
        {
            var response = await _bookService.GetBooksAsync();
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("availableBooks")]
        [Authorize(Roles = "Admin, Librarian, Member")]
        public async Task<IActionResult> GetAvaialable()
        {
            var response = await _bookService.GetAvailableBooksAsync();
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Librarian, Member")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(ApiResponse<BookDto>.Failure("Invalid book ID."));
            }

            var response = await _bookService.GetBookByIdAsync(id);

            return response.IsSuccess ? Ok(response) : NotFound(response);
        }

        [HttpGet("search")]
        [Authorize(Roles = "Admin, Librarian, Member")]
        public async Task<IActionResult> GetBySearch([FromQuery] SearchBookDto search)
        {
            var response = await _bookService.SearchBooksAsync(search);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> Create([FromBody] CreateBookDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Failure("Invalid book data."));

            try
            {
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                var response = await _bookService.CreateBookAsync(request, user);

                if (!response.IsSuccess)
                {
                    return StatusCode(500, response);
                }

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = response.Data?.Id },
                    response);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<BookDto>.Failure("An unexpected error occurred."));
            }
        }

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
                var response = await _bookService.BulkCreateBooksAsync(requests, user);

                return response.IsSuccess ? Ok(response) : BadRequest(response);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred during bulk creation."));
            }
        }

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
                var response = await _bookService.UpdateBookAsync(id, request, user);

                return response.IsSuccess ? Ok(response) : NotFound(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failure($"An error occurred while updating the book: {ex.Message}"));
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> UpdateStatus(string id, [FromQuery] bool isAvailable)
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
            var response = await _bookService.UpdateStatusAsync(id, isAvailable, user);

            return response.IsSuccess ? Ok(response) : NotFound(response);
        }

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
                var response = await _bookService.DeleteBookAsync(id, user);

                return response.IsSuccess ? Ok(response) : NotFound(response);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Failure("An unexpected error occurred while deleting the book."));
            }
        }
    }




}


