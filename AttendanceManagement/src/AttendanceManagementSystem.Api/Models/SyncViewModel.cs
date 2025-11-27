
namespace AttendanceManagementSystem.Api.Models 
{
    public class SyncViewModel
    {


        public string? Message { get; set; }
        // Agar muvaffaqiyatli bo'lsa, qancha log saqlanganini ko'rsatish uchun
        public int? SyncedCount { get; set; }
    }
}