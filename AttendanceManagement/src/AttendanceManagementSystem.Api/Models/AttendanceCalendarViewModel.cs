using AttendanceManagementSystem.Application.DTOs;

namespace AttendanceManagementSystem.Api.Models
{
    public class AttendanceCalendarViewModel
    {
        public long EmployeeId { get; set; }
        public string EmployeeFullName { get; set; }
        public DateTime TargetMonth { get; set; }
        public int TotalUnjustifiedLate { get; set; }
        public ICollection<CurrentAttendanceCalendar> MonthlyLogs { get; set; } = new List<CurrentAttendanceCalendar>();
    }
}
