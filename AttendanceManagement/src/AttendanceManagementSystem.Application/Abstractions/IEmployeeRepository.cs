using AttendanceManagementSystem.Domain.Entities;

namespace AttendanceManagementSystem.Application.Abstractions
{
    public interface IEmployeeRepository
    {
       
        Task<Employee?> GetEmployeeByIdAsync(long id);
        Task<Employee?> GetEmployeeByICCodeAsync(string code);
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
