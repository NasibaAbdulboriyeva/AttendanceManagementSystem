using AttendanceManagementSystem.Domain.Entities;

namespace AttendanceManagementSystem.Application.DTOs
{
    public class EmployeeScheduleDto
    {
        public long EmployeeId { get; set; }
        public long EmployeeScheduleId { get; set; }
        public int LimitInMinutes { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public EmployementType EmployementType { get; set; }
    }
}
