using AttendanceManagementSystem.Domain.Entities;

namespace AttendanceManagementSystem.Application.Abstractions
{
    public interface IEmployeeRepository
    {
       
        Task<Employee?> GetEmployeeByIdAsync(long id);
        Task<IReadOnlyDictionary<string, Employee>> GetEmployeesByUsernamesAsync(IReadOnlyCollection<string> usernames);
        Task<Employee?> GetEmployeeByUsernameAsync(string username);
        Task<long> AddEmployeeAsync(Employee employee);
        Task UpdateEmployeeAsync(Employee employee);
        Task<ICollection<Employee>> GetAllEmployeesAsync();
        Task<ICollection<Employee>> GetAllActiveEmployeesAsync();
        Task<EmployeeSchedule?> GetScheduleByEmployeeIdAsync(long employeeId);
        Task<ICollection<EmployeeSchedule>> GetAllSchedulesAsync();
        Task <long>AddEmployeeScheduleAsync(EmployeeSchedule schedule);
        Task UpdateScheduleAsync(EmployeeSchedule schedule);
        Task<int> SaveChangesAsync();
        Task<ICollection<long>> GetEmployeeIdsWithSchedulesAsync();
    }
}
