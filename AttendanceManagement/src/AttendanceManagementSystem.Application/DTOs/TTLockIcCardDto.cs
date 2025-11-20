

namespace AttendanceManagementSystem.Application.DTOs
{
    public class TTLockIcCardDto
    {
        public long CardId { get; set; } 
        public int LockId { get; set; }
        public string? CardNumber { get; set; } 
        public string? CardName { get; set; } 
        public int CardType { get; set; }
        public long StartDate { get; set; } 
        public long EndDate { get; set; } 
        public long CreateDate { get; set; } 
        public string? SenderUsername { get; set; }
    }
}
