using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Domain.Entities;
using DocumentFormat.OpenXml.Office2010.Excel;
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

        public async Task SetEmployeeInactiveAsync(string code)//agar ishdan ketsa 
        {
            var employee = await _context.Employees
                                  .FirstOrDefaultAsync(e => e.Code == code);
            if (employee != null)
            {
                employee.IsActive = false;
            }
            else
            {
                throw new Exception($"Employee with code {code} not found.");
            }
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

     
    
        public async Task AddRangeEmployeeScheduleAsync(ICollection<EmployeeSchedule> schedules)
        {
            if (schedules == null || schedules.Count == 0)
            {
                return;
            }

            await _context.EmployeeSchedules.AddRangeAsync(schedules);

            await _context.SaveChangesAsync();
        }

        public async Task<EmployeeSchedule?> GetScheduleByEmployeeIdAsync(long employeeId)
        {
 
            return await _context.EmployeeSchedules
                                 .FirstOrDefaultAsync(s => s.EmployeeId == employeeId);

        }

        public async Task UpdateScheduleByICCodeAsync(string code)
        {
            var schedule = await _context.EmployeeSchedules
                                         .Include(s => s.Employee) 
                                         .FirstOrDefaultAsync(s => s.Employee != null && s.Employee.Code == code);

            if (schedule == null)
            {
                return;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<ICollection<Employee>> GetAllActiveEmployeesAsync()
        {
            return await _context.Employees
                                .OrderBy(e => e.FullName)
                                .Where(e => e.IsActive)
                                .ToListAsync();

        }
    }
    }

