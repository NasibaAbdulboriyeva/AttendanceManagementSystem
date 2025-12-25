using AttendanceManagementSystem.Api.Configurations;
using AttendanceManagementSystem.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Infrastructure.Persistence.Repositories
{
    public class TTlockTokenRepository : ITTlockTokenRepository
    {
        private readonly AppDbContext _context;
        private const int SingleRecordId = 1;

        public TTlockTokenRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task InitializeTokenAsync(TTLockSettings initialToken)
        {
            var existingToken = await _context.TTlockSettings.FirstOrDefaultAsync(t => t.Id == SingleRecordId);

            if (existingToken == null)
            {
                initialToken.Id = SingleRecordId; 

                await _context.TTlockSettings.AddAsync(initialToken);
                await _context.SaveChangesAsync();

                Console.WriteLine("✅ TTLock boshlang'ich tokenlari Repository orqali bazaga saqlandi.");
            }
        }

        
        public async Task UpdateTokenAsync(TTLockSettings token)
        {
            var recordToUpdate = await _context.TTlockSettings.FirstOrDefaultAsync(t => t.Id == SingleRecordId);

            if (recordToUpdate == null)
            {
                throw new InvalidOperationException("TTLock token yozuvi bazada topilmadi. InitializeTokenAsync() avval chaqirilishi kerak.");
            }

            recordToUpdate.AccessToken = token.AccessToken;
            recordToUpdate.RefreshToken = token.RefreshToken;
            recordToUpdate.ExpiresAt = token.ExpiresAt;
           
            await _context.SaveChangesAsync();

            Console.WriteLine($"🔄 TTLock tokeni bazada muvaffaqiyatli yangilandi. Yangi amal qilish muddati: {token.ExpiresAt.ToLocalTime()}");
        }

      
        public async Task<TTLockSettings> GetTokenRecordAsync()
        {
            return await _context.TTlockSettings.FirstOrDefaultAsync(t => t.Id == SingleRecordId);
        }
    }
}