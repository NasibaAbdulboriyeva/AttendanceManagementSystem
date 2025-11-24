using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.Services;
using AttendanceManagementSystem.Infrastructure.Persistence.Gateway;
using AttendanceManagementSystem.Infrastructure.Persistence.Repositories;

namespace AttendanceManagementSystem.Api.Configurations
{
    public static class DependencyInjectionConfigurations
    {
        public static void ConfigureDI(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddScoped<IAttendanceLogRepository, AttendanceLogRepository>();
            builder.Services.AddScoped<IDoorActivityLogRepository, DoorActivityLogRepository>();
            builder.Services.AddScoped<IEmployeeSummaryRepository, EmployeeSummaryRepository>();
            //builder.Services.AddScoped<IUploadService, UploadService>();
            builder.Services.AddScoped<IAttendanceLogService, AttendanceLogService>();
            builder.Services.AddHttpClient<ITTLockService, TTLockService>();
            builder.Services.Configure<TTLockSettings>(
            builder.Configuration.GetSection("TTLockApiSettings"));

        }
    }
}
