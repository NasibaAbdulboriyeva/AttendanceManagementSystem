namespace AttendanceManagementSystem.Api.Models
{
    public class EmployeeSummary
    {
        public long EmployeeId { get; set; }
        public string FullName { get; set; }
     
        public int TotalLateMinutes { get; set; } = 0;
    }
}
