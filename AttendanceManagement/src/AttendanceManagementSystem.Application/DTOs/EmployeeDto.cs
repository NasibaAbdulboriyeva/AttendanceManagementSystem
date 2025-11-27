

namespace AttendanceManagementSystem.Application.DTOs
{
    public class EmployeeDto
    {
        public long EmployeeId { get; set; }
        public string UserName { get; set; }
        public bool IsActive { get; set; }
        public int? CardId { get; set; }
        public int? FingerprintId { get; set; }
        public string? FingerprintNumber { get; set; }
        public string? CardNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
