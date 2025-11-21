//using AttendanceManagementSystem.Application.Abstractions;
//using AttendanceManagementSystem.Application.DTOs;
//using AttendanceManagementSystem.Domain.Entities;


//namespace AttendanceManagementSystem.Application.Services
//{
//    public class EmployeeService : IEmployeeService
//    {
//        private readonly IEmployeeRepository _employeeRepository;

//        public EmployeeService(IEmployeeRepository employeeRepository)
//        {
//            _employeeRepository = employeeRepository;
//        }

//        public async Task<long> AddEmployeeAsync(EmployeeCreateDto employeeCreateDto)
//        {
//            var newEmployee = new Employee
//            {
//                IsActive = true,
//                CreatedAt = DateTime.Now
//            };

//            await _employeeRepository.AddEmployeeAsync(newEmployee);

//            return newEmployee.EmployeeId;
//        }

//        public async Task<EmployeeDto?> GetEmployeeByICCodeAsync(string code)
//        {
//            var employee = await _employeeRepository.GetEmployeeByICCodeAsync(code);

//            if (employee == null)
//            {
//                throw new Exception("emploee not found");
//            }

//            var employeeDto = new EmployeeDto
//            {
//                EmployeeId = employee.EmployeeId,
//                Code = employee.Code,
//                FullName = employee.FullName,
//                IsActive = employee.IsActive
//            };

//            return employeeDto;
//        }

//        public async Task<ICollection<EmployeeDto>> GetActiveEmployeesAsync()
//        {
//            var employees = await _employeeRepository.GetAllActiveEmployeesAsync();

//            return employees.Select(e => new EmployeeDto
//            {
//                EmployeeId = e.EmployeeId,
//                Code = e.Code,
//                FullName = e.FullName,
//                IsActive = e.IsActive
//            }).ToList();
//        }

//        public async Task<ICollection<EmployeeDto>> GetAllEmployeesAsync()
//        {
//            var employees = await _employeeRepository.GetAllEmployeesAsync();

//            return employees.Select(e => new EmployeeDto
//            {
//                EmployeeId = e.EmployeeId,
//                Code = e.Code,
//                FullName = e.FullName,
//                IsActive = e.IsActive
//            }).ToList();
//        }

//        public async Task DeactivateEmployeeAsync(string code)
//        {
//            var employee = await _employeeRepository.GetEmployeeByICCodeAsync(code);

//            if (employee == null)
//            {
//                throw new KeyNotFoundException($"Xodim {code} kodi bo'yicha topilmadi.");
//            }

//            employee.IsActive = false;
//            employee.ModifiedAt = DateTime.Now;

//            await _employeeRepository.UpdateEmployeeAsync(employee);
//        }

//        public async Task UpdateScheduleByICCodeAsync(string code)
//        {
//            if (code != null)
//            {
//                await _employeeRepository.UpdateScheduleByICCodeAsync(code);
//            }
//            else
//            {
//                throw new Exception("Invalid code");
//            }
//        }


//        }
//    }
