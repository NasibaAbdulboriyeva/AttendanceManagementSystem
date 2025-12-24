using AttendanceManagementSystem.Application.DTOs;

namespace AttendanceManagementSystem.Api.Models
{
    public class AttendanceCalendarViewModel
    {
        public string EmployeeFullName { get; set; }
        public DateTime TargetMonth { get; set; }
        public int TotalUnjustifiedLates { get; set; }
        public int DefaultLimit { get; set; }
        public ICollection<CurrentAttendanceCalendar> MonthlyLogs { get; set; } = new List<CurrentAttendanceCalendar>();
        public bool IsCalendarAlreadyCreatedForTargetMonth { get; set; }
    }
}
