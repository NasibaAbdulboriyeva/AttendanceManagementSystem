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

        // ---------------- LOGIN (GET) ----------------
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

        // ... (yuqoridagi kodlar)

        // ---------------- LOGIN (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserLoginDto model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // --- ASOSIY O'ZGARTIRISH: Service endi to'g'ridan-to'g'ri Claims ni qaytaradi ---
                var claims = await _authService.LoginUserAsync(model);

                if (claims == null || !claims.Any()) // claims null bo'lishi mumkin (login/parol xato bo'lsa)
                {
                    ModelState.AddModelError(string.Empty, "Login yoki parol noto‘g‘ri.");
                    return View(model);
                }

                // Claims listi Service qatlamidan kelganligi uchun, bu yerda Claimslarni 
                // qayta yaratish shart emas, balki ularni to'g'ridan-to'g'ri ishlatish kerak:
                var identity = new ClaimsIdentity(
                    claims, // Service dan kelgan claims ishlatildi
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
                ModelState.AddModelError(string.Empty, "Tizimga kirishda kutilmagan xato yuz berdi.");
                return View(model);
            }
        }
        // ... (qolgan kodlar o'zgarishsiz)
        // ---------------- SIGNUP (GET) ----------------
        [HttpGet]
        public IActionResult SignUp()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectByRole();

            return View(new UserCreateDto());
        }

        // ---------------- SIGNUP (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(UserCreateDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _authService.SignUpUserAsync(model);

                TempData["SuccessMessage"] = "Ro‘yxatdan muvaffaqiyatli o‘tdingiz. Iltimos, hisobingizga kiring.";
                return RedirectToAction(nameof(Login));
            }
            
            catch (Exception ex)
            {
                // Service qatlamida UserName allaqachon mavjudligi kabi xatolarni ushlash
                ModelState.AddModelError(string.Empty, $"Ro'yxatdan o'tishda xato: {ex.Message}");
                return View(model);
            }
        }

        // ---------------- LOGOUT ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Cookie ni o'chirish
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(nameof(Login));
        }

        // ---------------- HELPERS ----------------
        private IActionResult RedirectByRole()
        {
            // Foydalanuvchi ma'lumotlaridagi 'Role' claimiga asoslanadi.
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "AdminAttendance");

            }

            // Agar u Employee yoki boshqa bo'lsa, yoki Role aniqlanmagan bo'lsa, 'Home' ga yo'naltirish
            return RedirectToAction("Dashboard", "AdminAttendance");
        }
    }
}