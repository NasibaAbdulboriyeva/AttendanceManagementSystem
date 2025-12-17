using AttendanceManagementSystem.Application.DTOs.Auth;
using AttendanceManagementSystem.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AttendanceManagementSystem.Api.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
           
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {

                return RedirectToAction("Dashboard", "AdminAttendance");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new UserLoginDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserLoginDto model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                
                var claims = await _authService.LoginUserAsync(model);

                if (claims == null || !claims.Any()) 
                {
                    ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
                    return View(model);
                }

                
                var identity = new ClaimsIdentity(
                    claims, 
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    authProperties
                );

                if (Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectByRole();
            }
           
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Произошла ошибка при входе в систему.");
                return View(model);
            }
        }
    
        [HttpGet]
        public IActionResult SignUp()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectByRole();

            return View(new UserCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(UserCreateDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _authService.SignUpUserAsync(model);

                TempData["SuccessMessage"] = "Вы успешно зарегистрировались. Пожалуйста, войдите в свой аккаунт.";
                return RedirectToAction(nameof(Login));
            }
            
            catch (Exception ex)
            {
                // Service qatlamida UserName allaqachon mavjudligi kabi xatolarni ushlash
                ModelState.AddModelError(string.Empty, $"Ошибка при регистрации: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
           
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(nameof(Login));
        }

        private IActionResult RedirectByRole()
        {
       
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "AdminAttendance");

            }
           
            return RedirectToAction("Dashboard", "AdminAttendance");
        }
    }
}