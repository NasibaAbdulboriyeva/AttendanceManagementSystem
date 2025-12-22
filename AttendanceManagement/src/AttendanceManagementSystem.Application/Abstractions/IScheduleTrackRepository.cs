using AttendanceManagementSystem.Domain.Entities;

namespace AttendanceManagementSystem.Application.Abstractions
{
    public interface IScheduleTrackRepository
    {
        Task <long> AddScheduleHistoryAsync (EmployeeScheduleHistory scheduleHistory);
        Task <ICollection<EmployeeScheduleHistory>> GetScheduleHistoryByEmployeeIdAsync (long employeeId);
        Task <ICollection<EmployeeScheduleHistory>> GetScheduleByDateAndByEmployeeIdAsync(long employeeId,DateOnly targetDate);
    }
}
