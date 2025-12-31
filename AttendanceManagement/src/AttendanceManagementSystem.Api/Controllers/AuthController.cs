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
        private readonly ILogger<AuthController> _logger;

        // Constants
        private const string ACTION_LOGIN = nameof(Login);
        private const string ACTION_DASHBOARD = "Dashboard";
        private const string CONTROLLER_ADMIN = "AdminAttendance";

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Agar user allaqachon login bo'lgan bo'lsa
            if (User.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("Already authenticated user redirected to dashboard");
                return RedirectToAction(ACTION_DASHBOARD, CONTROLLER_ADMIN);
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
            {
                _logger.LogWarning("Invalid model state for login attempt with username: {Username}", model?.UserName);
                return View(model);
            }

            // Null check
            if (model == null || string.IsNullOrWhiteSpace(model.UserName))
            {
                _logger.LogWarning("Login attempt with null or empty username");
                ModelState.AddModelError(string.Empty, "❌ Логин обязателен.");
                return View(model);
            }

            try
            {
                var claims = await _authService.LoginUserAsync(model);

                if (claims == null || !claims.Any())
                {
                    _logger.LogWarning("Failed login attempt for username: {Username}", model.UserName);
                    ModelState.AddModelError(string.Empty, "❌ Неверный логин или пароль.");
                    return View(model);
                }

                // ClaimsIdentity yaratish
                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                // Cookie sozlamalari
                var authProperties = new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
                    AllowRefresh = true,
                    IssuedUtc = DateTimeOffset.UtcNow,
                    IsPersistent = true // Cookie browser yopilganda ham saqlanadi
                };

                // Sign In
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    authProperties
                );

                _logger.LogInformation("User {Username} logged in successfully at {Time}",
                    model.UserName, DateTime.UtcNow);

                // ReturnUrl tekshirish (Open Redirect himoyasi)
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction(ACTION_DASHBOARD, CONTROLLER_ADMIN);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized login attempt for user {Username}", model.UserName);
                ModelState.AddModelError(string.Empty, "❌ Неверный логин или пароль.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for user {Username}", model.UserName);
                ModelState.AddModelError(string.Empty, "❌ Произошла ошибка при входе в систему. Пожалуйста, попробуйте позже.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            // Agar user allaqachon login bo'lgan bo'lsa
            if (User.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation("Already authenticated user redirected to dashboard from signup");
                return RedirectToAction(ACTION_DASHBOARD, CONTROLLER_ADMIN);
            }

            return View(new UserCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(UserCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for signup attempt with username: {Username}", model?.UserName);
                return View(model);
            }

            // Null check
            if (model == null || string.IsNullOrWhiteSpace(model.UserName))
            {
                _logger.LogWarning("Signup attempt with null or empty username");
                ModelState.AddModelError(string.Empty, "❌ Имя пользователя обязательно.");
                return View(model);
            }

            try
            {
                await _authService.SignUpUserAsync(model);

                _logger.LogInformation("New user {Username} registered successfully at {Time}",
                    model.UserName, DateTime.UtcNow);

                TempData["SuccessMessage"] = "✅ Вы успешно зарегистрировались. Пожалуйста, войдите в свой аккаунт.";
                return RedirectToAction(ACTION_LOGIN);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User {Username} already exists", model.UserName);
                ModelState.AddModelError(string.Empty, "❌ Пользователь с таким именем уже существует.");
                return View(model);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid data provided during registration for {Username}", model.UserName);
                ModelState.AddModelError(string.Empty, $"❌ {ex.Message}");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {Username}", model.UserName);
                ModelState.AddModelError(string.Empty, "❌ Произошла ошибка при регистрации. Пожалуйста, попробуйте позже.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name ?? "Unknown";

            try
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                _logger.LogInformation("User {Username} logged out at {Time}",
                    username, DateTime.UtcNow);

                TempData["InfoMessage"] = "✅ Вы успешно вышли из системы.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {Username}", username);
                TempData["ErrorMessage"] = "❌ Произошла ошибка при выходе.";
            }

            return RedirectToAction(ACTION_LOGIN);
        }

        [HttpGet]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            _logger.LogWarning("Access denied. User: {Username}, ReturnUrl: {ReturnUrl}",
                User.Identity?.Name ?? "Anonymous", returnUrl);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}