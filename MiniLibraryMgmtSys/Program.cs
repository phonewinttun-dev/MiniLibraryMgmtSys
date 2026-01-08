using Microsoft.EntityFrameworkCore;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.Services;
using Serilog;
using Serilog.Sinks.MSSqlServer;

try { 
    var builder = WebApplication.CreateBuilder(args);

    Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/mini_library_mgmt_sys_log.txt", rollingInterval: RollingInterval.Hour)
    .WriteTo.MSSqlServer(
        connectionString: builder.Configuration.GetConnectionString("LogDbConnection"),
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = "Tbl_LogEvent",
            AutoCreateSqlTable = true,
            AutoCreateSqlDatabase = true
        })
    .CreateLogger();

    builder.Services.AddSerilog();
    // Add services to the container.

    builder.Services.AddControllers();
    //User Service Registration
    builder.Services.AddScoped<UserService>();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();


    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"));
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}


