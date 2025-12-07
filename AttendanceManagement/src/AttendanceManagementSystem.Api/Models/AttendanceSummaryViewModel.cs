namespace AttendanceManagementSystem.Api.Models
{
    public class AttendanceSummaryViewModel
    {
        public DateTime TargetMonth { get; set; } = DateTime.Now;

        // Dictionary olib tashlandi. Endi faqat EmployeeSummary ro'yxati ishlatiladi.
        public ICollection<EmployeeSummary> Employees { get; set; }
    }
}
