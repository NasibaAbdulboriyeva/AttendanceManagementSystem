using AttendanceManagementSystem.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Application.Services
{
    public interface IAuthService
    {
        Task<List<Claim>?> LoginUserAsync(UserLoginDto userLoginDto);

        Task<UserDto> SignUpUserAsync(UserCreateDto userCreateDto);

    }
}
