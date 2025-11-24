
using AttendanceManagementSystem.Domain.Entities;

namespace AttendanceManagementSystem.Application.DTOs
{
    public class CalculatedDailyLogDto
    {
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeFullName { get; set; }
        public TimeSpan ScheduledStartTime { get; set; }
        public int LateMinutesTotal { get; set; }
        public int RemainingLateMinutes { get; set; }
        public string? Description { get; set; }
        public bool IsJustified { get; set; }// ogohlatrlganmi
        public DateTime CalculatedAt { get; set; }
        public TimeOnly FirstEntryTime { get; set; }
        public TimeOnly LastLeavingTime { get; set; }//ketish vaqt 
        public int WorkedHours { get; set; }//kuniga qancha ishlagani 
        public DateOnly EntryDay { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
