using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs.Auth;
using AttendanceManagementSystem.Application.Services.Security;
using AttendanceManagementSystem.Domain.Entities;
using FluentValidation;
using System.Security.Claims;

namespace AttendanceManagementSystem.Application.Services
{

    public class AuthService : IAuthService
    {
        private readonly IUserRepository UserRepository;
        private readonly IValidator<UserCreateDto> UserValidator;
        private readonly IValidator<UserLoginDto> UserLoginValidator;

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
                return null;
            }

            // Parolni tekshirish
            var checkUserPassword = PasswordHasher.Verify(userLoginDto.Password, user.Password, user.Salt);

            if (checkUserPassword == false)
            {
                return null; 
            }

           
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.GivenName, user.FirstName), // Shaxsiy ism
                 
            };

            return claims;
        }

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
    }

    public static class Convert
    {
        public static UserDto ToUserDto(User user)
        {
            return new UserDto { UserId = user.UserId, UserName = user.UserName, FirstName = user.FirstName, LastName = user.LastName, PhoneNumber = user.PhoneNumber, Email = user.Email };
        }

        public static User ToUser(UserCreateDto dto, string passwordHash, string salt)
        {
            return new User { UserName = dto.UserName, Password = passwordHash, Salt = salt, FirstName = dto.FirstName, LastName = dto.LastName, PhoneNumber = dto.PhoneNumber, Email = dto.Email };
        }
    }
}