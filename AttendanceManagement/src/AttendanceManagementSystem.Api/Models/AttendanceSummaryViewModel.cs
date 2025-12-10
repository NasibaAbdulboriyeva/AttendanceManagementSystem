namespace AttendanceManagementSystem.Api.Models
{
    public class AttendanceSummaryViewModel
    {
        public DateTime TargetMonth { get; set; } = DateTime.Now;

     
        public ICollection<EmployeeSummary> Employees { get; set; }
    }
}
