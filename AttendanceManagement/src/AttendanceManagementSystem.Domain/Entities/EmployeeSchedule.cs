namespace AttendanceManagementSystem.Domain.Entities
{
    public class EmployeeSchedule
    {
        public long EmployeeScheduleId { get; set; }
        public long EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int LimitInMinutes { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public EmployementType EmployementType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

    }
}
