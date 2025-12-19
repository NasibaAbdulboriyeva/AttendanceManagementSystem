using AttendanceManagementSystem.Domain.Entities;

namespace AttendanceManagementSystem.Application.DTOs
{
    public class ScheduleHistoryDto
    {
        public long EmployeeId { get; set; }
        public int LimitInMinutes { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo{ get; set; }
        public EmployementType EmployementType { get; set; }
       
    }
}
