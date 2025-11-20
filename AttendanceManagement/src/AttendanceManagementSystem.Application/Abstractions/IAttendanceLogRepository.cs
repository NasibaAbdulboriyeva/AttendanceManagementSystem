using AttendanceManagementSystem.Domain.Entities;
namespace AttendanceManagementSystem.Application.Abstractions
{
    public interface IAttendanceLogRepository
    {
        Task AddLogsAsync(ICollection<AttendanceLog> logs);
        Task<ICollection<AttendanceLog>> GetLogsByEmployeeIdAsync(long employeeId, DateTime startDate, DateTime endDate);
        Task<ICollection<AttendanceLog>> GetLogsForAllEmployeesByDayAsync(DateTime targetDate);
        Task<ICollection<AttendanceLog>> GetLogsForEmployeeAndPeriodAsync( long employeeId, DateTime startDat,DateTime endDate);
    }
}
