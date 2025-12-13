using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs.Auth;
using AttendanceManagementSystem.Application.Services.Security;
using AttendanceManagementSystem.Domain.Entities;
using FluentValidation;
using System.Security.Claims;// ValidationException uchun

namespace AttendanceManagementSystem.Application.Services
{

    // Eslatma: API uchun yozilgan ITokenService va IRefreshTokenRepository
    // bu yerda ishlatilmagani uchun ularni o'chirish yoki shunchaki qoldirish mumkin.
    // Men ularni Dependency Injection konstruktorida qoldirdim, ammo metodlarda ishlatmadim.

    public class AuthService : IAuthService
    {
        private readonly IUserRepository UserRepository;
        private readonly IValidator<UserCreateDto> UserValidator;
        private readonly IValidator<UserLoginDto> UserLoginValidator;

        // JWT bilan bog'liq servislarni olib tashladim, yoki agar mavjud bo'lsa, ulardan foydalanishni to'xtatdim.
        public AuthService(
            IUserRepository userRepository,
            IValidator<UserCreateDto> userValidator,
            IValidator<UserLoginDto> userLoginValidator)
        {
            UserRepository = userRepository;
            UserValidator = userValidator;
            UserLoginValidator = userLoginValidator;
        }

        // --- 1. LOGIN MANTIQI (MVC uchun moslashtirildi) ---
        // Endi LoginResponseDto o'rniga Claims ro'yxatini qaytaradi.
        public async Task<List<Claim>?> LoginUserAsync(UserLoginDto userLoginDto)
        {
            var validationResult = await UserLoginValidator.ValidateAsync(userLoginDto);

            if (!validationResult.IsValid)

            {

                throw new ValidationException(validationResult.Errors);

            }

            var user = await UserRepository.SelectUserByUserNameAsync(userLoginDto.UserName);

            if (user == null)
            {
                // Xavfsizlik uchun umumiy xato xabarini qaytaramiz (null)
                // Controller bu null holatini xato deb qabul qiladi.
                return null;
            }

            // Parolni tekshirish
            var checkUserPassword = PasswordHasher.Verify(userLoginDto.Password, user.Password, user.Salt);

            if (checkUserPassword == false)
            {
                return null; // Parol noto'g'ri bo'lsa
            }

            // --- Muvaffaqiyatli kirish: Claims yaratish ---
            // TokenService o'rniga, to'g'ridan-to'g'ri Claimslarni qaytaramiz
            var claims = new List<Claim>
            {
                // Barcha muhim ma'lumotlar ClaimTypes orqali o'tkaziladi
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.GivenName, user.FirstName), // Shaxsiy ism
                // new Claim(ClaimTypes.Role, user.Role), // Agar foydalanuvchida Rol bo'lsa
            };

            return claims;
        }

        // --- 2. RO'YXATDAN O'TISH MANTIQI (Token yaratish olib tashlandi) ---
        // Endi faqat UserDto ni qaytaradi (yoki faqat boolean)
        public async Task<UserDto> SignUpUserAsync(UserCreateDto userCreateDto)
        {
            var validationResult = await UserValidator.ValidateAsync(userCreateDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException("Ro'yxatdan o'tish ma'lumotlari xato.", validationResult.Errors);
            }

            // Duplikat UserName ni tekshirish
            var existingUser = await UserRepository.SelectUserByUserNameAsync(userCreateDto.UserName);
            if (existingUser != null)
            {
                // Buni Repository qatlami bajarishi kerak, lekin xizmatda tekshirish ham yaxshi.
                throw new Exception("Foydalanuvchi nomi band qilingan.");
            }

            var tupleFromHasher = PasswordHasher.Hasher(userCreateDto.Password);

            var user = Convert.ToUser(userCreateDto, tupleFromHasher.Hash, tupleFromHasher.Salt);

            // Foydalanuvchini bazaga kiritish
            var userId = await UserRepository.InsertUserAsync(user);

            var userEntityWithId = await UserRepository.SelectUserByIdAsync(userId);

            // Controllerga muvaffaqiyatli yaratilgan UserDto ni qaytarish
            return Convert.ToUserDto(userEntityWithId);
        }



        /*
        // --- API Token mantig'i MVC uchun olib tashlandi/keraksiz ---

        // public Task<LoginResponseDto> RefreshTokenAsync(RefreshRequestDto request) { ... }
        // public Task LogOutAsync(string token) { ... }
        // private static RefreshToken CreateRefreshToken(string token, long userId) { ... }

        */

    }

    // Convert klasini o'zgarishsiz qoldiramiz (Entity mapping uchun kerak)
    public static class Convert
    {
        // User (Entity) -> UserGetDto
        public static UserDto ToUserDto(User user)
        {
            return new UserDto { UserId = user.UserId, UserName = user.UserName, FirstName = user.FirstName, LastName = user.LastName, PhoneNumber = user.PhoneNumber, Email = user.Email };
        }

        // UserCreateDto -> User (Entity)
        public static User ToUser(UserCreateDto dto, string passwordHash, string salt)
        {
            return new User { UserName = dto.UserName, Password = passwordHash, Salt = salt, FirstName = dto.FirstName, LastName = dto.LastName, PhoneNumber = dto.PhoneNumber, Email = dto.Email };
        }
    }
}