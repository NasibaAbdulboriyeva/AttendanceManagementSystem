using AttendanceManagementSystem.Application.DTOs;

namespace AttendanceManagementSystem.Application.Services
{
    public interface ICurrentAttendanceLogCalculationService
    {
        Task<ICollection<CurrentAttendanceCalendar>> GetAndSaveMonthlyAttendanceCalendarAsync(long employeeId, DateTime month);
        Task<ICollection<CurrentAttendanceCalendar>> GetMonthlyAttendanceCalendarsAsync(long employeeId, DateTime month);
        Task<int> GetLateArrivalsForPeriodAsync(long employeeId, DateTime month);
        Task ProcessAllEmployeesMonthlyAttendanceAsync(DateTime month);
        Task ProcessUpdateForAllEmployeesMonthlyAttendanceAsync(DateOnly month);
        int CalculateLateMinutes(CurrentAttendanceCalendar calendarDto, DateTime targetDate);
        Task<Dictionary<long, int>> GetEmployeesLateSummaryAsync(DateTime month);
        Task UpdateMonthlyEntryTimesAsync(long employeeId, DateOnly month);
        Task<int> UpdateEntryTimeManuallyAsync(UpdateEntryTimeDto dto);
        Task<bool> HasMonthlyAttendanceLogs(DateTime month);
        Task<Dictionary<long, int>> GetEmployeesDailyLateSummaryAsync(DateOnly day);
        Task<DateTime?> GetLastAttendanceLogDate(DateTime targetMonth);
        Task<int> UpdateJustificationStatusAsync(UpdateJustificationDto dto);
        Task<int> UpdateWorkingDayStatusAsync(WorkingDayStatusUpdateDto dto);
        Task<string?> UpdateDescriptionAsync(DescriptionUpdateDto dto);

    }
}
