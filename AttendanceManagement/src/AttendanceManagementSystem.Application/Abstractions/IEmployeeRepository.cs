using AttendanceManagementSystem.Domain.Entities;

namespace AttendanceManagementSystem.Application.Abstractions
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetEmployeeByICCodeAsync(string code);
        Task<Employee?> GetEmployeeByIdAsync(long id);
        Task<long> AddEmployeeAsync(Employee employee);
        Task UpdateEmployeeAsync(Employee employee);
        Task UpdateEmployeeScheduleAsync(EmployeeSchedule schedule);
        Task<ICollection<Employee>> GetAllEmployeesAsync();
    }
}
