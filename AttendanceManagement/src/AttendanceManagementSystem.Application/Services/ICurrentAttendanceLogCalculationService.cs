using AttendanceManagementSystem.Application.DTOs;

namespace AttendanceManagementSystem.Application.Services
{
    public interface ICurrentAttendanceLogCalculationService
    {
        Task<ICollection<CurrentAttendanceCalendar>> GetAndSaveMonthlyAttendanceCalendarAsync(long employeeId, DateTime month);
        Task<ICollection<CurrentAttendanceCalendar>> GetMonthlyAttendanceCalendarsAsync(long employeeId, DateTime month);
        Task<ICollection<CurrentAttendanceCalendar>> GetLateArrivalsForPeriodAsync(string employeeCode, DateTime month);
        Task ProcessAllEmployeesMonthlyAttendanceAsync(DateTime month);
        int CalculateLateMinutes(CurrentAttendanceCalendar calendarDto, DateTime targetDate);
        Task UpdateEntryTimeManuallyAsync(UpdateEntryTimeDto dto);
        Task UpdateJustificationStatusAsync(UpdateJustificationDto dto);

    }
}
