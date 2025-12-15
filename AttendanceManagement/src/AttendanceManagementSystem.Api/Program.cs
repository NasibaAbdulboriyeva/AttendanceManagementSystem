using AttendanceManagementSystem.Api.Configurations;
using AttendanceManagementSystem.Application.Abstractions;

namespace AttendanceManagementSystem.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.ConfigureDB();
            builder.ConfigureDI();



            builder.Services.AddAuthentication("Cookies")
          .AddCookie("Cookies", options =>
           {
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
                    logger.LogError(ex, "Kritik xato: TTLock tokenlarini initsializatsiya qilishda muammo yuz berdi.");
                   
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
                pattern: "{controller=Auth}/{action=SignUp}/{id?}");

            app.Run();
        }
    }
}
