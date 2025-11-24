using AttendanceManagementSystem.Application.DTOs;

namespace AttendanceManagementSystem.Application.Services
{
    public interface IAttendanceCalculationService
    {
        Task<ICollection<CalculatedDailyLogDto>> GetCalculatedAttendanceForPeriodAsync(
            string employeeCode,
            DateTime startDate,
            DateTime endDate);
        Task<DateTime?> GetFirstEntryTimeForEmployeeByDayAsync(long employeeId, DateTime targetDate);
        Task<ICollection<CalculatedDailyLogDto>> GetLateArrivalsForPeriodAsync(
            string employeeCode,
            DateTime startDate,
            DateTime endDate);

        Task<EmployeeSummaryDto> CalculateAndSaveMonthlySummaryAsync(
            string employeeCode,
            int year,
            int month);

        Task<RemainingLimitDto> CheckRemainingLateLimitAsync(
            string employeeCode,
            int year,
            int month);
    }
}
