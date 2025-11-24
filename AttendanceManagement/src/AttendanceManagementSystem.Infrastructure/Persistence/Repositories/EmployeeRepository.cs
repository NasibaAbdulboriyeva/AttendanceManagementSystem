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
                                 .OrderBy(e => e.UserName)
                                 .ToListAsync();
        }
       

        public async  Task<Employee?> GetEmployeeByIdAsync(long id)
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
        //public async Task SetEmployeeInactiveAsync(string code)//agar ishdan ketsa 
        //{
        //    var employee = await _context.Employees
        //                          .FirstOrDefaultAsync(e => e.Code == code);
        //    if (employee != null)
        //    {
        //        employee.IsActive = false;
        //    }
        //    else
        //    {
        //        throw new Exception($"Employee with code {code} not found.");
        //    }
        //}

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
      
        // --- ✅ YANGI OPTIMALLASHTIRILGAN METOD ---
        /// <summary>
        /// Berilgan foydalanuvchi ismlari ro'yxati bo'yicha barcha xodimlarni bir marta, ommaviy so'rovda yuklaydi.
        /// </summary>
        /// <param name="usernames">Qidiriladigan foydalanuvchi ismlarining to'plami.</param>
        /// <returns>Kalit (Key) sifatida UserName va Qiymat (Value) sifatida Employee obyektiga ega bo'lgan lug'at (Dictionary).</returns>
        public async Task<IReadOnlyDictionary<string, Employee>> GetEmployeesByUsernamesAsync(IReadOnlyCollection<string> usernames)
        {
            if (usernames == null || !usernames.Any())
            {
                return new Dictionary<string, Employee>(StringComparer.OrdinalIgnoreCase);
            }

            // 1. EF Core yordamida LINQ so'rovi (SQL ga WHERE IN ga aylanadi)
            var employees = await _context.Employees
                .Where(e => usernames.Contains(e.UserName))
                .ToListAsync();

            // 2. Natijani tezkor qidiruv uchun Dictionary (lug'at) ga konvertatsiya qilish
            // UserName maydonini kalit (key) sifatida ishlatamiz.
            // StringComparer.OrdinalIgnoreCase harf registrini inobatga olmaslik uchun ishlatiladi, 
            // bu foydalanuvchi ismlarida muhim bo'lishi mumkin.
            return employees.ToDictionary(
                e => e.UserName,
                e => e,
                StringComparer.OrdinalIgnoreCase
            );
        }

        // ... Boshqa CRUD yoki Custom Repositry metodlar shu yerda bo'lishi kerak
    

    //public async Task UpdateScheduleByICCodeAsync(string code)
    //{
    //    var schedule = await _context.EmployeeSchedules
    //                                 .Include(s => s.Employee) 
    //                                 .FirstOrDefaultAsync(s => s.Employee != null && s.Employee.Code == code);

    //    if (schedule == null)
    //    {
    //        return;
    //    }
    //    await _context.SaveChangesAsync();
    //}

    public async Task<ICollection<Employee>> GetAllActiveEmployeesAsync()
        {
            return await _context.Employees
                                .OrderBy(e => e.UserName)
                                .Where(e => e.IsActive)
                                .ToListAsync();

        }

        public Task SetEmployeeInactiveAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task UpdateScheduleByICCodeAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<Employee?> GetEmployeeByICCodeAsync(string code)
        {
            throw new NotImplementedException();
        }
    }
    }

