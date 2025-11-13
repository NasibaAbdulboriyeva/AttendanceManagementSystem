using AttendanceManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace AttendanceManagementSystem.Web.Configurations
{
    public static class DatabaseConfiguration
    {
        public static void ConfigureDB(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");

            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString).LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
           .EnableSensitiveDataLogging());
        }
    }
}
