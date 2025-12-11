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
        private readonly IRefreshTokenRepository RefreshTokenRepository;
        private readonly IUserRepository UserRepository;
        private readonly ITokenService TokenService;
        private readonly IValidator<UserCreateDto> UserValidator;
        private readonly IValidator<UserLoginDto> UserLoginValidator;

        public AuthService(IRefreshTokenRepository refreshTokenRepository, IUserRepository userRepository, ITokenService tokenService, IValidator<UserCreateDto> userValidator, IValidator<UserLoginDto> userLoginValidator)
        {
            RefreshTokenRepository = refreshTokenRepository;
            UserRepository = userRepository;
            TokenService = tokenService;
            UserValidator = userValidator;
            UserLoginValidator = userLoginValidator;
        }

        // 1. LOGIN MANTIQI
        public async Task<LoginResponseDto> LoginUserAsync(UserLoginDto userLoginDto)
        {
            var validationResult = await UserLoginValidator.ValidateAsync(userLoginDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var user = await UserRepository.SelectUserByUserNameAsync(userLoginDto.UserName);

            // Foydalanuvchi topilmaganini tekshirish muhim
            if (user == null)
            {
                throw new Exception("UserName or password incorrect"); // Xavfsizlik uchun bir xil xato xabari
            }

            // Parolni tekshirish
            var checkUserPassword = PasswordHasher.Verify(userLoginDto.Password, user.Password, user.Salt);

            if (checkUserPassword == false)
            {
                throw new Exception("UserName or password incorrect");
            }

            // ✅ MapUser o'rniga Convert.ToUserDto ishlatildi
            var userGetDto = Convert.ToUserDto(user);

            var accessToken = TokenService.GenerateToken(userGetDto);

            var refreshToken = CreateRefreshToken(Guid.NewGuid().ToString(), user.UserId);

            await RefreshTokenRepository.InsertRefreshTokenAsync(refreshToken);

            var loginResponseDto = new LoginResponseDto()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                TokenType = "Bearer",
                Expires = 25,
            };

            return loginResponseDto;
        }

        // 2. TOKENNI YANGILASH MANTIQI
        public async Task<LoginResponseDto> RefreshTokenAsync(RefreshRequestDto request)
        {
            ClaimsPrincipal? principal = TokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null) throw new Exception("Invalid access token.");

            var userClaim = principal.FindFirst(c => c.Type == "UserId");

            // Claim mavjudligini tekshirish muhim
            if (userClaim == null) throw new Exception("Missing User ID claim in access token.");

            var userId = long.Parse(userClaim.Value);

            // Refresh tokenni topish
            var refreshToken = await RefreshTokenRepository.SelectRefreshTokenAsync(request.RefreshToken, userId);

            // Tokenning haqiqiyligini tekshirish
            if (refreshToken == null || refreshToken.Expires < DateTime.UtcNow || refreshToken.IsRevoked)
                throw new Exception("Invalid or expired refresh token.");

            // Eski refresh tokenni bekor qilish (Revoke)
            refreshToken.IsRevoked = true;
            await RefreshTokenRepository.UpdateRefreshTokenAsync(refreshToken); // ✅ Update metodida yangi expirationDate berilmasligi kerak, chunki Revoke bo'lmoqda. Repository da IsRevoked yangilansa yetarli.

            var user = await UserRepository.SelectUserByIdAsync(userId);
            if (user == null) throw new Exception("User not found for refresh operation.");

            // ✅ MapUser o'rniga Convert.ToUserDto ishlatildi
            var userGetDto = Convert.ToUserDto(user);

            var newAccessToken = TokenService.GenerateToken(userGetDto);

            // TokenService'dan foydalanish o'rniga statik metodga o'zgartirildi, chunki eski kodingiz shunday edi
            var newRefreshToken = CreateRefreshToken(Guid.NewGuid().ToString(), user.UserId); // TokenService.GenerateRefreshToken() o'rniga statik metodga o'zgartirildi

            // Yangi refresh tokenni DB ga kiritish
            await RefreshTokenRepository.InsertRefreshTokenAsync(newRefreshToken);

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                TokenType = "Bearer",
                Expires = 25,
            };
        }

        // 3. RO'YXATDAN O'TISH MANTIQI
        // Qaytarish turi LoginResponseDto ga o'zgartirildi
        public async Task<LoginResponseDto> SignUpUserAsync(UserCreateDto userCreateDto)
        {
            var validationResult = await UserValidator.ValidateAsync(userCreateDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var tupleFromHasher = PasswordHasher.Hasher(userCreateDto.Password);

            // ✅ MapUser o'rniga Convert.ToUser ishlatildi
            var user = Convert.ToUser(userCreateDto, tupleFromHasher.Hash, tupleFromHasher.Salt);

            // Foydalanuvchini bazaga kiritish
            var userId = await UserRepository.InsertUserAsync(user);

            var userEntityWithId = await UserRepository.SelectUserByIdAsync(userId);

            var refreshToken = CreateRefreshToken(Guid.NewGuid().ToString(), userId);

            await RefreshTokenRepository.InsertRefreshTokenAsync(refreshToken);

            // Access Token yaratish
            var accessToken = TokenService.GenerateToken(Convert.ToUserDto(userEntityWithId));

            // ✅ Ro'yxatdan o'tgandan keyin tokenlarni qaytarish
            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                TokenType = "Bearer",
                Expires = 25,
            };
        }

        // 4. LOGOUT MANTIQI
        public async Task LogOutAsync(string token)
        {
            await RefreshTokenRepository.RemoveRefreshTokenAsync(token);
        }

        // 5. Yordamchi metod (Tuzatilgan)
        private static RefreshToken CreateRefreshToken(string token, long userId)
        {
            return new RefreshToken
            {
                Token = token,
                // ✅ Expir o'rniga ExpirationDate ishlatildi, Repository'ga moslashish uchun
                Expires = DateTime.UtcNow.AddDays(21),
                IsRevoked = false,
                UserId = userId
            };
        }

      
    }

    public static class Convert
    {
        // User (Entity) -> UserGetDto
        public static UserDto ToUserDto(User user)
        {
            // Haqiqiy Konvertatsiya mantig'i shu yerda bo'ladi
            return new UserDto { UserId = user.UserId, UserName = user.UserName , FirstName = user.FirstName, LastName = user.LastName, PhoneNumber = user.PhoneNumber, Email = user.Email };
        }

        // UserCreateDto -> User (Entity)
        public static User ToUser(UserCreateDto dto, string passwordHash, string salt)
        {
            // Haqiqiy Konvertatsiya mantig'i shu yerda bo'ladi
            return new User { UserName = dto.UserName, Password = passwordHash, Salt = salt, FirstName=dto.FirstName,LastName=dto.LastName,PhoneNumber=dto.PhoneNumber,Email=dto.Email};

        }
    }
}