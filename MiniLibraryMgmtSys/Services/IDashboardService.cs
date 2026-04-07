using MiniLibraryMgmtSys.DTO;
using System.Threading.Tasks;

namespace MiniLibraryMgmtSys.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    }
}
