using MiniLibraryMgmtSys.Database.AppDbContextModels;

namespace MiniLibraryMgmtSys.Infrastructure
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(TblUser user);
    }
}
