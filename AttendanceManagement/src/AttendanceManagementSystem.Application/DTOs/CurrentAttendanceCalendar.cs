namespace AttendanceManagementSystem.Application.DTOs
{
    public class CurrentAttendanceCalendar
    {
        public long EmployeeId { get; set; }
        public string? EmployeeFullName { get; set; }
        public TimeOnly ScheduledStartTime { get; set; }
        public int LateMinutesTotal { get; set; }
        public int RemainingLateMinutes { get; set; }
        public string? Description { get; set; }
        public bool IsJustified { get; set; }
        public bool IsWorkingDay { get; set; }
        public DateTime CalculatedAt { get; set; }
        public TimeOnly FirstEntryTime { get; set; }
        public TimeOnly LastLeavingTime { get; set; }
        public int WorkedHours { get; set; }
        public DateOnly EntryDay { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
