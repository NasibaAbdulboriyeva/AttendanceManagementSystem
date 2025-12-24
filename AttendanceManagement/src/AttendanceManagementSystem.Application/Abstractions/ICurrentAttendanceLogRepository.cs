using AttendanceManagementSystem.Domain.Entities;
namespace AttendanceManagementSystem.Application.Abstractions
{
    public interface ICurrentAttendanceLogRepository
    {
      
        Task<ICollection<CurrentAttendanceLog>> GetAllLogsAsync();
        Task<ICollection<CurrentAttendanceLog>> GetLogsByEmployeeIdAsync(long employeeId);
        Task<ICollection<CurrentAttendanceLog>> GetLogsByEmployeeIdByMonthAsync(long employeeId, DateOnly month);
        Task<CurrentAttendanceLog>GetLogByEmployeeIdAndEntryDayAsync(long employeeId,DateOnly date);
        Task<ICollection<CurrentAttendanceLog>>CreateLogAsync(ICollection<CurrentAttendanceLog> logs);
        Task<CurrentAttendanceLog> UpdateLogAsync(CurrentAttendanceLog log);
        Task UpdateRangeAsync(IEnumerable<CurrentAttendanceLog> logs);
        Task<bool> HasMonthlyAttendanceLogs(DateTime month);
        Task<int> GetLateArrivalsForPeriodAsync(long employeeId, DateTime month);
        Task<int> GetLateArrivalsForDayAsync(long employeeId, DateOnly day);
      
        Task<ICollection<CurrentAttendanceLog>> GetLogsWithoutEntryTimeAsync(long employeeId, DateOnly month);
        Task<int> SaveChangesAsync();

    }
}
