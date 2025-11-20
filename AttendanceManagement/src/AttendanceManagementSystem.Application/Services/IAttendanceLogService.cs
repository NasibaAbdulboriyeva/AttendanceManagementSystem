using AttendanceManagementSystem.Domain.Entities;

namespace AttendanceManagementSystem.Application.Services
{
    public interface IAttendanceLogService
    {
        Task<int> SyncAttendanceLogsAsync(int lockId,DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
        Task<ICollection<AttendanceLog>> GetLogsByEmployeeIdAsync(int employeeId, DateTime startDate, DateTime endDate);

        Task<ICollection<AttendanceLog>> GetLogsForAllEmployeesByDayAsync(DateTime targetDate);//usha kunga tegishlisini olish

        Task<ICollection<AttendanceLog>> GetLogsForEmployeeAndPeriodAsync(long employeeId, DateTime startDate, DateTime endDate);
    }
}
