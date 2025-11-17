using AttendanceManagementSystem.Domain.Entities;

namespace AttendanceManagementSystem.Application.Abstractions
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetEmployeeByICCodeAsync(string code);
        Task<Employee?> GetEmployeeByIdAsync(long id);
        Task<long> AddEmployeeAsync(Employee employee);
        Task SetEmployeeInactiveAsync(string code);
        Task UpdateEmployeeAsync(Employee employee);
        Task<ICollection<Employee>> GetAllEmployeesAsync();
        Task<ICollection<Employee>> GetAllActiveEmployeesAsync();
        Task<EmployeeSchedule?> GetScheduleByEmployeeIdAsync(long employeeId);
        Task AddRangeEmployeeScheduleAsync(ICollection<EmployeeSchedule> schedules);
        Task UpdateScheduleByICCodeAsync(string code);
    }
}
