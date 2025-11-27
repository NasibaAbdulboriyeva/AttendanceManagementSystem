using AttendanceManagementSystem.Application.DTOs;
namespace AttendanceManagementSystem.Application.Services
{
    public interface IEmployeeService
    {
        Task<ICollection<EmployeeDto>> GetAllEmployeesAsync();
        Task<ICollection<EmployeeDto>> GetAllActiveEmployeesAsync();
        Task<EmployeeDto?> GetEmployeeByIdAsync(long id);
        Task DeactivateEmployeeAsync(long id);
        Task<(int CardsSynced, int FingerprintsSynced)> SyncEmployeeDataAsync();
        Task<EmployeeScheduleDto?> GetEmployeeScheduleByEmployeeIdAsync(long employeeId);
        Task UpdateEmployeeScheduleAsync( EmployeeScheduleDto scheduleDto);
        Task<long> AddEmployeeScheduleAsync(EmployeeScheduleDto scheduleDto);
        Task<long> GetEmployeeIdByUsernameAsync(string username);
    }
}
