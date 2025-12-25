namespace AttendanceManagementSystem.Domain.Entities
{
    public class Employee
    {
        public long EmployeeId { get; set; }
        public string UserName { get; set; }
        public bool IsActive { get; set; }
        public int? CardId { get; set; }
        public int? FingerprintId { get; set; }
        public string? FingerprintNumber{ get; set; }
        public string? CardNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public ICollection<AttendanceLog> AttendanceLogs { get; set; }
        public ICollection<CurrentAttendanceLog> CurrentAttendanceLogs { get; set; }
        public ICollection<EmployeeScheduleHistory> EmployeeScheduleHistories { get; set; }
        public EmployeeSchedule EmployeeSchedule { get; set; }
    }
}
