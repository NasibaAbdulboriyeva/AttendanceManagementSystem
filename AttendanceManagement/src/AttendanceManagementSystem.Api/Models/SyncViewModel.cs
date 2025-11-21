using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Api.Models 
{
    public class SyncViewModel
    {
        [Required(ErrorMessage = "Iltimos, Lock ID kiriting.")]
        [Display(Name = "LockId number")]
        [Range(1, int.MaxValue, ErrorMessage = "ID musbat son bo'lishi kerak.")]
        public int LockId { get; set; }

        // Natijani ekranga chiqarish uchun
        public string? Message { get; set; }

        // Agar muvaffaqiyatli bo'lsa, qancha log saqlanganini ko'rsatish uchun
        public int? SyncedCount { get; set; }
    }
}