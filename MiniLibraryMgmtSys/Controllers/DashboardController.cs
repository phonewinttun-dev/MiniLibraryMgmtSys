using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.DTO;
using MiniLibraryMgmtSys.Infrastructure;
using MiniLibraryMgmtSys.Services;
using System;
using System.Threading.Tasks;

namespace MiniLibraryMgmtSys.Controllers
{
    [Authorize(Roles = "Admin, Librarian")]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetSummary()
        {
            try
            {
                var summary = await _dashboardService.GetDashboardSummaryAsync();
                return Ok(ApiResponse<DashboardSummaryDto>.Success(summary, "Dashboard summary retrieved successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<DashboardSummaryDto>.Failure($"An error occurred while retrieving the dashboard summary: {ex.Message}"));
            }
        }
    }
}
