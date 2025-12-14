using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs.Auth;
using AttendanceManagementSystem.Application.Services;
using AttendanceManagementSystem.Application.Validators;
using AttendanceManagementSystem.Infrastructure.Persistence.Gateway;
using AttendanceManagementSystem.Infrastructure.Persistence.Repositories;
using AttendanceManagementSystem.Infrastructure.Persistence.Repositories.AttendanceManagementSystem.Application.Abstractions;
using FluentValidation;

namespace AttendanceManagementSystem.Api.Configurations
{
    public static class DependencyInjectionConfigurations
    {
        public static void ConfigureDI(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddScoped<IAttendanceLogRepository, AttendanceLogRepository>();
            builder.Services.AddScoped<IAttendanceLogService, AttendanceLogService>();
            builder.Services.AddScoped<ITTlockTokenRepository, TTlockTokenRepository>();
            builder.Services.AddScoped<ICurrentAttendanceLogRepository, CurrentAttendanceLogRepository>();
            builder.Services.AddScoped<ICurrentAttendanceLogCalculationService, CurrentAttendanceLogCalculationService>();


            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
       
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<ITokenService, TokenService>();

            builder.Services.AddScoped<IValidator<UserCreateDto>, UserCreateValidator>();
            builder.Services.AddScoped<IValidator<UserLoginDto>, UserLoginValidator>();
         
            //builder.Services.AddScoped<IUploadService, UploadService>();
            //builder.Services.AddScoped<IDoorActivityLogRepository, DoorActivityLogRepository>();
            builder.Services.AddHttpClient<ITTLockService, TTLockService>();
            builder.Services.Configure<TTLockSettings>(
            builder.Configuration.GetSection("TTLockApiSettings"));

        }
    }
}
