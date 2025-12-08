using AttendanceManagementSystem.Application.DTOs;
using AttendanceManagementSystem.Domain.Entities;

namespace AttendanceManagementSystem.Application.Services
{
    public interface ICurrentAttendanceLogCalculationService
    {
        Task<ICollection<CurrentAttendanceCalendar>> GetAndSaveMonthlyAttendanceCalendarAsync(long employeeId, DateTime month);
        Task<ICollection<CurrentAttendanceCalendar>> GetMonthlyAttendanceCalendarsAsync(long employeeId, DateTime month);
        Task<int> GetLateArrivalsForPeriodAsync(long employeeId, DateTime month);
        Task ProcessAllEmployeesMonthlyAttendanceAsync(DateTime month);
        int CalculateLateMinutes(CurrentAttendanceCalendar calendarDto, DateTime targetDate);
        Task<Dictionary<long, int>> GetEmployeesLateSummaryAsync(DateTime month);
        Task<int> UpdateEntryTimeManuallyAsync(UpdateEntryTimeDto dto);
        Task<int> UpdateJustificationStatusAsync(UpdateJustificationDto dto);
        Task<int> UpdateWorkingDayStatusAsync(WorkingDayStatusUpdateDto dto);
        Task<string?> UpdateDescriptionAsync(DescriptionUpdateDto dto);

    }
}
