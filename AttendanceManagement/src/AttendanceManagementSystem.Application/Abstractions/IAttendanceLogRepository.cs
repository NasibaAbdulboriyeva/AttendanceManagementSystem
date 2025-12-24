using AttendanceManagementSystem.Domain.Entities;

namespace AttendanceManagementSystem.Application.Abstractions
{
    public interface IAttendanceLogRepository
    {
        Task AddLogsAsync(ICollection<AttendanceLog> logs);
        Task<ICollection<AttendanceLog>> GetLogsByEmployeeAndMonthAsync(long employeeId, DateTime month );
        Task<ICollection<AttendanceLog>> GetLogsForAllEmployeesByDayAsync(DateTime targetDate);
        Task<ICollection<AttendanceLog>> GetLogsForEmployeeAndPeriodAsync( long employeeId, DateTime startDate,DateTime endDate);
        Task<DateTime?> GetLastRecordedTimeAsync();
        Task<Dictionary<DateOnly, TimeOnly>> GetMonthlyFirstEntryTimesAsync(long employeeId, DateOnly month);

        Task<ICollection<AttendanceLog>> GetLogsByEmployeeAndDateAsync(long employeeId, DateTime date);
    }
}
