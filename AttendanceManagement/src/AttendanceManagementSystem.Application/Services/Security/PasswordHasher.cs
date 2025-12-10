namespace AttendanceManagementSystem.Application.Services.Security
{
    public class PasswordHasher
    {
        // 1. Hashlash (BCrypt o'zi saltni yaratadi va natijaga qo'shadi)
        public static (string Hash, string Salt) Hasher(string password)
        {
            // HashPassword() metodi o'zi avtomatik tarzda xavfsiz saltni yaratib, parolni unga qo'shib hashlagan holda qaytaradi.
            var hash = BCrypt.Net.BCrypt.HashPassword(password);


            return (Hash: hash, Salt: string.Empty); // Salt bo'sh qolishi mumkin, agar DB talab qilmasa
        }

        // 2. Tekshirish (BCrypt Verify metodi hash ichidagi saltni o'zi topadi)
        public static bool Verify(string password, string hash, string salt)
        {
            // Verify metodida faqat ochiq parolni va hashni berish kifoya.
            // Saltni qo'shish shart emas, chunki hash ichida u mavjud.
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
