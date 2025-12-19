namespace AttendanceManagementSystem.Domain.Entities
{
    public class EmployeeSchedule
    {
        public long EmployeeScheduleId { get; set; }
        public long EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int LimitInMinutes { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public EmployementType EmployementType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

    }
}
