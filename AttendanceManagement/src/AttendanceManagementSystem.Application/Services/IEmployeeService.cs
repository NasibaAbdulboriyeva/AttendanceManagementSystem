using AttendanceManagementSystem.Application.DTOs;
namespace AttendanceManagementSystem.Application.Services
{
    public interface IEmployeeService
    {
        Task<long> AddEmployeeAsync(EmployeeCreateDto employeeCreateDto);
        Task<ICollection<EmployeeDto>> GetAllEmployeesAsync();
        Task<ICollection<EmployeeDto>> GetActiveEmployeesAsync();
        Task<EmployeeDto?> GetEmployeeByICCodeAsync(string code);
        Task DeactivateEmployeeAsync(string code);
        Task UpdateScheduleByICCodeAsync(string code);
        Task<(int CardsSynced, int FingerprintsSynced)> SyncEmployeeDataAsync();
    }
}
