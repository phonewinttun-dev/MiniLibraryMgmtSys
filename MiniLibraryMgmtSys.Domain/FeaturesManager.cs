using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.Domain.Features.Auth;
using MiniLibraryMgmtSys.Domain.Features.Book;
using MiniLibraryMgmtSys.Domain.Features.Borrow;
using MiniLibraryMgmtSys.Domain.Features.Dashboard;
using MiniLibraryMgmtSys.Domain.Features.User;


namespace MiniLibraryMgmtSys.Domain
{
    public static class FeaturesManager
    {
        public static void AddDomain(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"));
            });
            //Book Service Registration
            builder.Services.AddScoped<IBookService, BookService>();
            //User Service Registration
            builder.Services.AddScoped<IUserService, UserService>();
            //Auth Service Registration
            builder.Services.AddScoped<IAuthService, AuthService>();
            //Borrow Service Registration
            builder.Services.AddScoped<IBorrowService, BorrowService>();
            //Dashboard Service Registration
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            //JWT Tokens Registration
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        }
    }
}
