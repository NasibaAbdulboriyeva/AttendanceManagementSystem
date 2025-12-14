using AttendanceManagementSystem.Api.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Application.Abstractions
{
    public interface ITTlockTokenRepository
    {
        Task InitializeTokenAsync(TTLockSettings initialToken);

        Task UpdateTokenAsync(TTLockSettings token);
        Task<TTLockSettings> GetTokenRecordAsync();
    }
}