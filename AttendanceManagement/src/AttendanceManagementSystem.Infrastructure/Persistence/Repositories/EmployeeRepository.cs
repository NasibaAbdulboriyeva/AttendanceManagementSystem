
using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagementSystem.Infrastructure.Persistence.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;
        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<long> AddEmployeeAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee.EmployeeId;
        }

        public async Task<ICollection<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees
                                 .OrderBy(e => e.FullName)
                                 .ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByICCodeAsync(string code)
        {
            var employee = await _context.Employees
                                 .FirstOrDefaultAsync(e => e.Code == code);
            return employee;
        }

        public async  Task<Employee?> GetEmployeeByIdAsync(long id)
        {
            var employee = await _context.Employees
                                  .FirstOrDefaultAsync(e => e.EmployeeId == id);
            return employee;
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEmployeeScheduleAsync(EmployeeSchedule schedule)
        {

            var existingSchedule = await _context.EmployeeSchedules.FirstOrDefaultAsync(s => s.EmployeeId == schedule.EmployeeId);

            if (existingSchedule == null)
            {
                _context.EmployeeSchedules.Add(schedule);
            }
            else
            {
                _context.EmployeeSchedules.Update(schedule);
            }

            await _context.SaveChangesAsync();
        }
    }
}
