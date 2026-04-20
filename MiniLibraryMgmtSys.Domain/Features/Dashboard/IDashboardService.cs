using MiniLibraryMgmtSys.Infrastructure;
using MiniLibraryMgmtSys.DTO;
using System.Threading.Tasks;

namespace MiniLibraryMgmtSys.Domain.Features.Dashboard
{
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardSummaryDto>> GetDashboardSummaryAsync();
    }
}
