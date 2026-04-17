using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.DTO;
using MiniLibraryMgmtSys.Infrastructure;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MiniLibraryMgmtSys.Domain.Features.Borrow
{
    [ApiController]
    [Route("api/borrow")]
    [Authorize(Roles = "Member")]
    public class BorrowController : ControllerBase
    {
        private readonly IBorrowService _borrowService;

        public BorrowController(IBorrowService borrowService)
        {
            _borrowService = borrowService;
        }

        [HttpPost("borrow")]
        public async Task<IActionResult> Borrow([FromBody] BorrowRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _borrowService.BorrowBookAsync(userId, request.BookId);

            if (result == null)
                return BadRequest(ApiResponse<BorrowResponseDto>.Failure("Failed to borrow book."));

            return Ok(ApiResponse<BorrowResponseDto>.Success(result, "Book borrowed successfully."));
        }

        [HttpPost("return")]
        public async Task<IActionResult> Return([FromBody] BorrowRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _borrowService.ReturnBookAsync(userId, request.BookId);

            if (!result)
                return BadRequest(ApiResponse<bool>.Failure("Failed to return book."));

            return Ok(ApiResponse<bool>.Success(true, "Book returned successfully."));
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _borrowService.GetUserBorrowingHistoryAsync(userId);
            return Ok(ApiResponse<List<BorrowResponseDto>>.Success(result, "Borrowing history retrieved successfully."));
        }

        [HttpGet("all-history")]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> GetAllHistory()
        {
            var result = await _borrowService.GetAllBorrowingHistoryAsync();
            return Ok(ApiResponse<List<BorrowResponseDto>>.Success(result, "All borrowing history retrieved successfully."));
        }
    }
}
