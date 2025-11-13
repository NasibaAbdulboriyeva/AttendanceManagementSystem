namespace AttendanceManagementSystem.Domain.Entities
{
    public class Employee
    {
        public long EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<AttendanceLog> AttendanceLogs { get; set; }
        public ICollection<EmployeeSummary> EmployeeSummaries { get; set; }
        public EmployeeSchedule EmployeeSchedule { get; set; }
    }
}
