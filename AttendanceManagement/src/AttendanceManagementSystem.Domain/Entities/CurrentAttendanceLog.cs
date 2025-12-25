namespace AttendanceManagementSystem.Domain.Entities
{
    public class CurrentAttendanceLog
    {
        public long CurrentAttendanceLogId { get; set; }
        public long EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int LateArrivalMinutes { get; set; }
        public int RemainingLateMinutes { get; set; }
        public string? Description { get; set; }
        public bool IsJustified { get; set; } 
        public bool IsWorkingDay { get; set; } 
        public DateTime  CalculatedAt { get; set; }
        public TimeOnly  FirstEntryTime { get; set; }
        public TimeOnly  ScheduledStartTime { get; set; }
        public TimeOnly LastLeavingTime { get; set; }
        public int WorkedHours { get; set; }
        public DateOnly EntryDay { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

    }
}
