using AttendanceManagementSystem.Application.DTOs;

namespace AttendanceManagementSystem.Application.Services
{
    public interface ICurrentAttendanceLogCalculationService
    {
        Task<ICollection<CurrentAttendanceCalendar>> GetCalculatedAttendanceForPeriodAsync(string employeeCode, DateTime startDate, DateTime endDate);
        Task<ICollection<CurrentAttendanceCalendar>> GetAndSaveMonthlyAttendanceCalendarAsync(long employeeId, DateTime month);
        Task<ICollection<CurrentAttendanceCalendar>> GetLateArrivalsForPeriodAsync(string employeeCode, DateTime month);

    }
}
