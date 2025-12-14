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

        // Dependency Injection orqali DbContextni qabul qilish
        public TTlockTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        // Boshlang'ich tokenlarni saqlash (Agar baza bo'sh bo'lsa)
        public async Task InitializeTokenAsync(TTLockSettings initialToken)
        {
            // 1. Bazada yozuv mavjudligini tekshirish
            var existingToken = await _context.TTlockSettings.FirstOrDefaultAsync(t => t.Id == SingleRecordId);

            if (existingToken == null)
            {
                initialToken.Id = SingleRecordId; 

                await _context.TTlockSettings.AddAsync(initialToken);
                await _context.SaveChangesAsync();

                Console.WriteLine("✅ TTLock boshlang'ich tokenlari Repository orqali bazaga saqlandi.");
            }
            // Agar yozuv mavjud bo'lsa, hech narsa qilmaymiz, eski tokenlarni saqlab qolamiz.
        }

        
        public async Task UpdateTokenAsync(TTLockSettings token)
        {
            // 1. Yangilanadigan yozuvni olish (Tracking ni yoqish uchun)
            var recordToUpdate = await _context.TTlockSettings.FirstOrDefaultAsync(t => t.Id == SingleRecordId);

            if (recordToUpdate == null)
            {
                throw new InvalidOperationException("TTLock token yozuvi bazada topilmadi. InitializeTokenAsync() avval chaqirilishi kerak.");
            }

            // 2. Olingan yozuvni yangi ma'lumotlar bilan yangilash
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