using AttendanceManagementSystem.Application.DTOs.Auth;
using AttendanceManagementSystem.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AttendanceManagementSystem.Api.Controllers
{
        // MVC Controller'lar uchun to'g'ri nomlash konvensiyasi
        public class AuthController : Controller
        {
            private readonly IAuthService _authService;

            public AuthController(IAuthService authService)
            {
                _authService = authService;
            }

            // --- 1. LOGIN (GET) ---
            // Tizimga kirish formasini ko'rsatadi
            [HttpGet]
            public IActionResult Login(string? returnUrl = null)
            {
                // Agar foydalanuvchi allaqachon kirgan bo'lsa, uni bosh sahifaga yo'naltirish
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Dashboard", "AdminAttendance");
                }
                ViewData["ReturnUrl"] = returnUrl;
                return View(new UserLoginDto());
            }

            // --- 2. LOGIN (POST) ---
            // Login ma'lumotlarini qabul qilib, foydalanuvchini kirgizadi
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Login(UserLoginDto model, string? returnUrl = null)
            {
                ViewData["ReturnUrl"] = returnUrl;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // --- AuthService'dan Login Mantig'ini Ishlatish ---
                try
                {
                    // Sizning AuthService.LoginUserAsync metodi AccessToken, RefreshToken qaytaradi (API uchun).
                    // Biz hozir bu metodning faqat birinchi qismini, ya'ni foydalanuvchi mavjudligi
                    // va parolni to'g'ri tekshirish qismini ishlatishimiz kerak.

                    var loginResponse = await _authService.LoginUserAsync(model);

                    // Muvaffaqiyatli kirish bo'lsa, JWT yaratilgan bo'ladi.
                    // Biz JWT yaratilishidan keyin, foydalanuvchini MVC Cookie orqali avtorizatsiya qilamiz.

                    if (loginResponse != null)
                    {
                        // Foydalanuvchi ma'lumotlaridan Claims yaratish
                        var claims = new List<Claim>
                    {
                        // Bu ma'lumotlar Cookie ichida saqlanadi
                        new Claim(ClaimTypes.NameIdentifier, model.UserName),
                        new Claim(ClaimTypes.Name, model.UserName),
                        // Agar rol ma'lumotlari bo'lsa, bu yerga qo'shiladi:
                        // new Claim(ClaimTypes.Role, "Admin"), 
                    };

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) // Cookie amal qilish muddati
                        };

                        // Cookie ni yaratish va brauzerga yuborish
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        // Oldingi sahifaga yoki Bosh sahifaga yo'naltirish
                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Dashboard", "AdminAttendance");
                    }

                    // Agar LoginUserAsync exception tashlamasa, lekin null qaytarsa (yoki login xato bo'lsa)
                    ModelState.AddModelError(string.Empty, "Foydalanuvchi nomi yoki parol noto'g'ri.");
                    return View(model);
                }
                catch (ValidationException)
                {
                    // AuthService ichidagi validatsiya xatolarini ushlab olish (agar model validatsiya qilinmasa)
                    ModelState.AddModelError(string.Empty, "Kirish ma'lumotlari formatida xato.");
                    return View(model);
                }
                catch (Exception)
                {
                    // Boshqa barcha xatolar
                    ModelState.AddModelError(string.Empty, "Tizimga kirishda xato yuz berdi.");
                    return View(model);
                }
            }

        [HttpGet]
        public IActionResult SignUp()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "AdminAttendance");
            }
            // UserCreateDto modeli View'ga yuboriladi
            return View(new UserCreateDto());
        }

        // ---------------------------------------------
        // --- 4. SIGNUP (POST) ---
        // Ro'yxatdan o'tish ma'lumotlarini qabul qilish
        // ---------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(UserCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // AuthService orqali foydalanuvchini yaratish va unga token olish
                // Sizning SignUpUserAsync metodi LoginResponseDto qaytaradi (avtomatik kirish/tokenlash)
                var response = await _authService.SignUpUserAsync(model);

                // Ro'yxatdan o'tish muvaffaqiyatli bo'lsa, foydalanuvchini Login sahifasiga yuborish
                // Yoki agar SignUpUserAsync muvaffaqiyatli kirishni ham amalga oshirsa, Dashboardga yuborish

                // Eng yaxshi usul: Ro'yxatdan o'tgandan keyin Login sahifasiga yo'naltirish
                TempData["SuccessMessage"] = "Ro'yxatdan muvaffaqiyatli o'tdingiz. Iltimos, hisobingizga kiring.";
                return RedirectToAction("Login");

                /*
                // Agar ro'yxatdan o'tgandan so'ng avtomatik kirishni istasangiz:
                // Yuqoridagi LOGIN (POST) mantig'ini shu yerda takrorlash kerak edi. 
                // Ammo Login sahifasiga yo'naltirish tozaroq yechim hisoblanadi.
                */
            }
           
            catch (Exception ex)
            {
                // Boshqa xatolar (masalan, UserName band bo'lsa)
                ModelState.AddModelError(string.Empty, $"Ro'yxatdan o'tishda xato: {ex.Message}");
                return View(model);
            }
        }
        // --- 3. LOGOUT (GET/POST) ---
        // Foydalanuvchini tizimdan chiqaradi
        [HttpGet]
            [HttpPost]
            public async Task<IActionResult> Logout()
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // JWT Refresh Tokendan ham xalos bo'lish kerak, agar kerak bo'lsa:
                // await _authService.LogOutAsync("Joriy_Refresh_Token_Bu_Yerdan_Olinadi"); 

                return RedirectToAction("Login", "Auth");
            }
        }
    }