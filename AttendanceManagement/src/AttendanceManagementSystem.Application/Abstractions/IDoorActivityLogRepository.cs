using AttendanceManagementSystem.Domain.Entities;
namespace AttendanceManagementSystem.Application.Abstractions
{
    public interface IDoorActivityLogRepository
    {
        Task AddDoorLogsAsync(ICollection<DoorActivityLog> logs);
        Task<ICollection<DoorActivityLog>> GetLogsByDayAsync(DateTime targetDate);
    }
}
