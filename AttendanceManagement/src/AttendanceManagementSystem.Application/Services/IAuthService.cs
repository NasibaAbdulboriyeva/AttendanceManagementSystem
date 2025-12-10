using AttendanceManagementSystem.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Application.Services
{
    public interface  IAuthService
    {
         Task<LoginResponseDto>SignUpUserAsync(UserCreateDto userCreateDto);
        Task<LoginResponseDto> LoginUserAsync(UserLoginDto userLoginDto);
        Task<LoginResponseDto> RefreshTokenAsync(RefreshRequestDto request);
        Task LogOutAsync(string token);
    }
}
