using System.Threading.Tasks;
using MiniLibraryMgmtSys.Domain.DTOs;
using MiniLibraryMgmtSys.Shared;

namespace MiniLibraryMgmtSys.Domain.Features.Dashboard
{
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardSummaryDto>> GetDashboardSummaryAsync();
    }
}
