namespace AttendanceManagementSystem.Domain.Entities
{
    public class Employee
    {
        public long EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public int CardId { get; set; }
        public int CardNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public ICollection<AttendanceLog> AttendanceLogs { get; set; }
        public ICollection<CurrentAttendanceLog> CurrentAttendanceLogs { get; set; }
        public EmployeeSchedule EmployeeSchedule { get; set; }
    }
}
