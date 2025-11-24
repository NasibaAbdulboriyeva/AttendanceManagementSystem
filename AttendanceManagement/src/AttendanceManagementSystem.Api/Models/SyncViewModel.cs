
namespace AttendanceManagementSystem.Api.Models 
{
    public class SyncViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        // Natijani ekranga chiqarish uchun
        public string? Message { get; set; }
        // Agar muvaffaqiyatli bo'lsa, qancha log saqlanganini ko'rsatish uchun
        public int? SyncedCount { get; set; }
    }
}