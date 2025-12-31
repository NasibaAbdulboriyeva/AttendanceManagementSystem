using AttendanceManagementSystem.Api.Configurations;
using AttendanceManagementSystem.Application.Abstractions;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AttendanceManagementSystem.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.ConfigureDB();
            builder.ConfigureDI();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
      .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => 
            {
               options.ExpireTimeSpan = TimeSpan.FromDays(7);
               options.SlidingExpiration = true;
               options.Cookie.HttpOnly = true;
               options.Cookie.SameSite = SameSiteMode.Lax; // ?
               options.LoginPath = "/Auth/Login";
               options.LogoutPath = "/Auth/Logout";
               options.AccessDeniedPath = "/Auth/AccessDenied";
           });

            builder.Services.AddAuthorization();
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var tokenService = services.GetRequiredService<ITTLockService>();
                    await tokenService.InitializeTokensFromConfigAsync();
                   
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Критическая ошибка: возникла проблема при инициализации токенов TTLock.");
                   
                }
            }
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Auth}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
