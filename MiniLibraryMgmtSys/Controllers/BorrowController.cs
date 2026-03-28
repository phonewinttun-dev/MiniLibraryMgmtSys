using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.DTO;
using MiniLibraryMgmtSys.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MiniLibraryMgmtSys.Controllers
{
    [ApiController]
    [Route("api/borrow")]
    [Authorize]
    public class BorrowController : ControllerBase
    {
        private readonly IBorrowService _borrowService;
        private readonly ILogger<BorrowController> _logger;

        public BorrowController(IBorrowService borrowService, ILogger<BorrowController> logger)
        {
            _borrowService = borrowService;
            _logger = logger;
        }

        [HttpPost("borrow")]
        public async Task<IActionResult> Borrow([FromBody] BorrowRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) 
                return Unauthorized();

            _logger.LogInformation("User {UserId} is borrowing book {BookId}", userId, request.BookId);
            var result = await _borrowService.BorrowBookAsync(userId, request.BookId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("return")]
        public async Task<IActionResult> Return([FromBody] BorrowRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            _logger.LogInformation("User {UserId} is returning book {BookId}", userId, request.BookId);
            var result = await _borrowService.ReturnBookAsync(userId, request.BookId);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _borrowService.GetUserBorrowingHistoryAsync(userId);
            return Ok(result);
        }

        [HttpGet("all-history")]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> GetAllHistory()
        {
            var result = await _borrowService.GetAllBorrowingHistoryAsync();
            return Ok(result);
        }
    }
}
