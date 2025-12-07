namespace AttendanceManagementSystem.Api.Models
{
    // EmployeeSummary.cs
    public class EmployeeSummary
    {
        public long EmployeeId { get; set; }
        public string FullName { get; set; }
        // ... Boshqa kerakli maydonlar (masalan, Department)

        // YANGI MAYDON: Joriy oy uchun umumiy kech qolish minutlari
        public int TotalLateMinutes { get; set; } = 0;
    }
}
