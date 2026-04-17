using MiniLibraryMgmtSys.DTO;
using System.Threading.Tasks;

namespace MiniLibraryMgmtSys.Domain.Features.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    }
}
