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
    }
}
