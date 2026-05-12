using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.Domain.DTOs;
using MiniLibraryMgmtSys.Shared;
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

            var response = await _borrowService.BorrowBookAsync(userId, request.BookId);

            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("return")]
        public async Task<IActionResult> Return([FromBody] BorrowRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var response = await _borrowService.ReturnBookAsync(userId, request.BookId);

            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var response = await _borrowService.GetUserBorrowingHistoryAsync(userId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("all-history")]
        [Authorize(Roles = "Admin, Librarian")]
        public async Task<IActionResult> GetAllHistory()
        {
            var response = await _borrowService.GetAllBorrowingHistoryAsync();
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
