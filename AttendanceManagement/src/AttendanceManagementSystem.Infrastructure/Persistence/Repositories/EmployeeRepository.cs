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
        public async Task<ICollection<long>> GetEmployeeIdsWithSchedulesAsync()
        {
            return await _context.EmployeeSchedules
                .AsNoTracking() 
                .Select(s => s.EmployeeId) 
                .Distinct() 
                .ToListAsync();
        }
        public async Task<ICollection<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees
                                 .OrderBy(e => e.UserName)
                                 .ToListAsync();
        }


        public async Task<Employee?> GetEmployeeByIdAsync(long id)
        {
            var employee = await _context.Employees
                                  .FirstOrDefaultAsync(e => e.EmployeeId == id);
            return employee;
        }
        public async Task<Employee?> GetEmployeeByUsernameAsync(string username)
        {
            var employee = await _context.Employees
                                  .FirstOrDefaultAsync(e => e.UserName == username);
            return employee;
        }


        public async Task UpdateEmployeeAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }



        public async Task<long> AddEmployeeScheduleAsync(EmployeeSchedule schedule)
        {

            await _context.EmployeeSchedules.AddAsync(schedule);
            await _context.SaveChangesAsync();
            return schedule.EmployeeScheduleId;
        }

        public async Task<EmployeeSchedule?> GetScheduleByEmployeeIdAsync(long employeeId)
        {

            return await _context.EmployeeSchedules
                                 .FirstOrDefaultAsync(s => s.EmployeeId == employeeId);

        }

        public async Task<IReadOnlyDictionary<string, Employee>> GetEmployeesByUsernamesAsync(IReadOnlyCollection<string> usernames)
        {
            if (usernames == null || !usernames.Any())
            {
                return new Dictionary<string, Employee>(StringComparer.OrdinalIgnoreCase);
            }

            var employees = await _context.Employees
                .Where(e => usernames.Contains(e.UserName))
                .ToListAsync();
            return employees.ToDictionary(
                e => e.UserName,
                e => e,
                StringComparer.OrdinalIgnoreCase
            );
        }



        public async Task UpdateScheduleAsync(EmployeeSchedule schedule)
        {
            _context.EmployeeSchedules.Update(schedule);
            schedule.ModifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();

        }

        public async Task<ICollection<Employee>> GetAllActiveEmployeesAsync()
        {
            return await _context.Employees
                                .OrderBy(e => e.UserName)
                                .Where(e => e.IsActive)
                                .ToListAsync();

        }

        public async Task<ICollection<EmployeeSchedule>> GetAllSchedulesAsync()
        {
            return await _context.EmployeeSchedules
                                  .OrderBy(e => e.Employee.UserName)
                                  .ToListAsync();
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();

        }
    }
    }

