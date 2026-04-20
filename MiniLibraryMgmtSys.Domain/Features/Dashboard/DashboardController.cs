using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibraryMgmtSys.DTO;
using MiniLibraryMgmtSys.Infrastructure;
using System;
using System.Threading.Tasks;

namespace MiniLibraryMgmtSys.Domain.Features.Dashboard
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
                var response = await _dashboardService.GetDashboardSummaryAsync();
                return response.IsSuccess ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<DashboardSummaryDto>.Failure($"An error occurred while retrieving the dashboard summary: {ex.Message}"));
            }
        }
    }
}
