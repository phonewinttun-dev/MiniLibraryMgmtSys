using MiniLibraryMgmtSys.Database.AppDbContextModels;

namespace MiniLibraryMgmtSys.Domain.Features.Auth
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(TblUser user);
    }
}
