using AttendanceManagementSystem.Domain.Entities;

namespace AttendanceManagementSystem.Application.Services
{
    public interface IAttendanceLogService
    {
        Task<int> SyncAttendanceLogsAsync(DateTimeOffset? startDate , DateTimeOffset? endDate);
        Task<ICollection<AttendanceLog>> GetLogsByEmployeeIdAsync(long employeeId, DateTime startDate, DateTime endDate);

        Task<ICollection<AttendanceLog>> GetLogsForAllEmployeesByDayAsync(DateTime targetDate);//usha kunga tegishlisini olish

    }
}
