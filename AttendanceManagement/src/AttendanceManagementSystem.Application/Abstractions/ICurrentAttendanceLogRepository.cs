using AttendanceManagementSystem.Domain.Entities;
namespace AttendanceManagementSystem.Application.Abstractions
{
    public interface ICurrentAttendanceLogRepository
    {
        Task<CurrentAttendanceLog> GetLogByIdAsync(long id);
        Task<ICollection<CurrentAttendanceLog>> GetAllLogsAsync();
        Task<ICollection<CurrentAttendanceLog>> GetLogsByEmployeeIdAsync(long employeeId);
        Task<ICollection<CurrentAttendanceLog>>CreateLogAsync(ICollection<CurrentAttendanceLog> logs);
        Task<CurrentAttendanceLog> UpdateLogAsync(CurrentAttendanceLog log);
        Task DeleteLogAsync(long id);

    }
}
