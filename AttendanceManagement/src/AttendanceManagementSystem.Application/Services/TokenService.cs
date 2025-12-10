using AttendanceManagementSystem.Application.DTOs.Auth;
using AttendanceManagementSystem.Application.Services.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace AttendanceManagementSystem.Application.Services
{

    public class TokenService : ITokenService
    {
        private IConfiguration Configuration;

        public TokenService(IConfiguration configuration)
        {
            Configuration = configuration.GetSection("Jwt");
        }

        public string GenerateToken(UserDto user)
        {
            var IdentityClaims = new Claim[]
            {
         new Claim("UserId",user.UserId.ToString()),
         new Claim("FirstName",user.FirstName.ToString()),
         new Claim("LastName",user.LastName.ToString()),
         new Claim("PhoneNumber",user.PhoneNumber.ToString()),
         new Claim("UserName",user.UserName.ToString()),
         new Claim(ClaimTypes.Role, user.Role.ToString()),
         new Claim(ClaimTypes.Email,user.Email.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecurityKey"]!));
            var keyCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresHours = int.Parse(Configuration["Lifetime"]);
            var token = new JwtSecurityToken(
                issuer: Configuration["Issuer"],
                audience: Configuration["Audience"],
                claims: IdentityClaims,
                expires: TimeHelper.GetDateTime().AddHours(expiresHours),
                signingCredentials: keyCredentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            // 64 bayt (48 ta Base64 belgisi) uzunlikdagi xavfsiz tasodifiy token yaratish
            var randomBytes = new byte[64];

            // RandomNumberGenerator.Create() metodi System.Security.Cryptography namespace'ida joylashgan
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            // Convert.ToBase64String metodi System.Convert sinfida joylashgan
            // Agar 'Convert' ishlamasa, uning oldiga 'System' qo'yish nom to'qnashuvini hal qiladi
            return System.Convert.ToBase64String(randomBytes);
        }
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Configuration["Issuer"],
                ValidateAudience = true,
                ValidAudience = Configuration["Audience"],
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecurityKey"]!))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
        }

        public string RemoveRefreshTokenAsync(string token)
        {
            throw new NotImplementedException("This method is not implemented ");
        }
    }
}
