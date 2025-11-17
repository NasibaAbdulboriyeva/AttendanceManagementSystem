

namespace AttendanceManagementSystem.Application.DTOs
{
    public class EmployeeDto
    {
        public long EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
