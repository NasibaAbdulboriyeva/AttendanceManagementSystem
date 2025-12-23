using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs;
using AttendanceManagementSystem.Domain.Entities;

namespace AttendanceManagementSystem.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ITTLockService _ttLockService;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IScheduleTrackRepository _scheduleTrackRepository;

        public EmployeeService(ITTLockService ttLockService, IEmployeeRepository employeeRepository,IScheduleTrackRepository scheduleTrackRepository)
        {
            _ttLockService = ttLockService;
            _employeeRepository = employeeRepository;
            _scheduleTrackRepository = scheduleTrackRepository;
        }

        public async Task<(int CardsSynced, int FingerprintsSynced)> SyncEmployeeDataAsync()
        {

            int cardsSynced = await SyncEmployeesByTypeAsync<TTLockIcCardDto>(
                (searchStr, orderBy) => _ttLockService.GetAllIcCardRecordsAsync(searchStr, orderBy),
                (employee, userDto) =>
                {
                    employee.CardId = userDto.CardId;
                    employee.CardNumber = userDto.CardNumber ?? string.Empty;
                    if (string.IsNullOrEmpty(employee.UserName))
                        employee.UserName = userDto.CardName ?? string.Empty;
                });

            int fingerprintsSynced = await SyncEmployeesByTypeAsync<TTLockFingerprintDto>(
                (searchStr, orderBy) => _ttLockService.GetAllFingerprintsPaginatedAsync(searchStr, orderBy),
                (employee, userDto) =>
                {
                    employee.FingerprintId = userDto.FingerprintId;
                    employee.FingerprintNumber = userDto.FingerprintNumber ?? string.Empty;
                    if (string.IsNullOrEmpty(employee.UserName))
                    {
                        employee.UserName = userDto.FingerprintName ?? string.Empty;

                    }

                });

            return (cardsSynced, fingerprintsSynced);
        }

        private async Task<int> SyncEmployeesByTypeAsync<T>(
            Func<string?, int, Task<ICollection<T>>> fetchFunc,
            Action<Employee, T> mapAction)
            where T : class
        {

            int syncedCount = 0;

            ICollection<T> ttLockUsers;
            try
            {

                ttLockUsers = await fetchFunc(null, 1);
            }
            catch (Exception ex)
            {
                return 0;
            }

            if (ttLockUsers == null || !ttLockUsers.Any())
            {
                return 0;
            }

            foreach (var ttUser in ttLockUsers)
            {
                Employee? employee = null;

                if (ttUser is TTLockIcCardDto icCardDto)
                {

                    if (icCardDto.CardId > 0)
                    {
                        employee = await _employeeRepository.GetEmployeeByUsernameAsync(icCardDto.CardName);
                    }
                }

                else if (ttUser is TTLockFingerprintDto fingerprintDto)
                {

                    if (fingerprintDto.FingerprintId > 0)
                    {
                        employee = await _employeeRepository.GetEmployeeByUsernameAsync(fingerprintDto.FingerprintName);
                    }
                }

                bool isNew = (employee == null);

                if (isNew)
                {
                    employee = new Employee
                    {
                        CreatedAt = DateTime.Now,
                        IsActive = true,
                        UserName = string.Empty,
                        CardNumber = string.Empty
                    };
                }

                mapAction(employee, ttUser);
                if (isNew)
                {
                    await _employeeRepository.AddEmployeeAsync(employee);
                }
                else
                {
                    employee.ModifiedAt = DateTime.Now;
                    await _employeeRepository.UpdateEmployeeAsync(employee);
                }
                syncedCount++;
            }

            return syncedCount;
        }

        public async Task DeactivateEmployeeAsync(long id)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id); // Kod orqali qidirish
            if (employee == null)
            {
                throw new KeyNotFoundException($"Сотрудник с идентификатором {id} не найден.");
            }

            employee.IsActive = false;
            employee.ModifiedAt = DateTime.UtcNow;
            await _employeeRepository.UpdateEmployeeAsync(employee);
        }


        public async Task<ICollection<EmployeeDto>> GetAllEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();
            return employees.Select(MapToDto).ToList();
        }
        public async Task<ICollection<EmployeeScheduleDto>> GetAllSchedulesAsync()
        {
            var employeeSchedules = await _employeeRepository.GetAllSchedulesAsync();
            return employeeSchedules.Select(MapToDto).ToList();
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(long id)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                throw new Exception($"Сотрудник с идентификатором {id} не найден.");
            }
            return MapToDto(employee);
        }

        public async Task<long> AddEmployeeScheduleAsync(EmployeeScheduleDto scheduleDto)
        {
            if (scheduleDto == null)
            {
                throw new ArgumentNullException(nameof(scheduleDto), "Данные таблицы не могут быть пустыми.");
            }

            if (scheduleDto.EmployeeId <= 0)
            {
                throw new ArgumentException("EmployeeId должен иметь корректное значение.", nameof(scheduleDto.EmployeeId));
            }

            var scheduleEntity = MapToEntity(scheduleDto);

            await _employeeRepository.AddEmployeeScheduleAsync(scheduleEntity);

            return scheduleEntity.EmployeeScheduleId;
        }
       
        public async Task<EmployeeScheduleDto?> GetEmployeeScheduleByEmployeeIdAsync(long employeeId)
        {
            var scheduleEntity = await _employeeRepository.GetScheduleByEmployeeIdAsync(employeeId);

            var scheduleDto = MapToDto(scheduleEntity);

            return scheduleDto;
        }

        public async Task UpdateEmployeeScheduleAsync(EmployeeScheduleDto scheduleDto)
        {
            if (scheduleDto == null)
            {
                throw new ArgumentNullException(nameof(scheduleDto), "Данные таблицы не могут быть пустыми");
            }

            var existingSchedule = await _employeeRepository.GetScheduleByEmployeeIdAsync(scheduleDto.EmployeeId);

            if (existingSchedule == null)
            {
                throw new KeyNotFoundException($"Для сотрудника {scheduleDto.EmployeeId} расписание не найдено. Его нужно создать заранее.");
            }
            var scheduleHistoryEntity = MapToHistory(existingSchedule);
            var employeeScheduleHistory = await _scheduleTrackRepository.AddScheduleHistoryAsync(scheduleHistoryEntity);

            existingSchedule.StartTime = scheduleDto.StartTime;
            existingSchedule.EndTime = scheduleDto.EndTime;

            await _employeeRepository.UpdateScheduleAsync(existingSchedule);
        }

        public async Task<long> GetEmployeeIdByUsernameAsync(string username)
        {
            var employee = await _employeeRepository.GetEmployeeByUsernameAsync(username);
            
            if (employee == null)
            {
                throw new KeyNotFoundException($"Сотрудник с именем пользователя '{username}' не найден.");
            }

            return employee.EmployeeId;
        }
        public EmployeeDto MapToDto(Employee employee)
        {
            return new EmployeeDto
            {
                EmployeeId = employee.EmployeeId,
                UserName = employee.UserName,
                CardId = employee.CardId,
                CardNumber = employee.CardNumber,
                FingerprintId = employee.FingerprintId,
                FingerprintNumber = employee.FingerprintNumber,
                IsActive = employee.IsActive,
                CreatedAt = employee.CreatedAt,
                ModifiedAt = employee.ModifiedAt
            };
        }
        public EmployeeSchedule MapToEntity(EmployeeScheduleDto dto)
        {
            return new EmployeeSchedule
            {
                EmployeeId = dto.EmployeeId,
                LimitInMinutes = dto.LimitInMinutes,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                EmployementType = dto.EmployementType,
                CreatedAt = DateTime.Now,
                ModifiedAt = default
            };
        }
        private Employee MapToEntity(EmployeeDto dto)
        { 
            return new Employee
            {
                UserName = dto.UserName,
                CardId = dto.CardId,
                CardNumber = dto.CardNumber,
                FingerprintId = dto.FingerprintId,
                FingerprintNumber = dto.FingerprintNumber,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
        }

        private EmployeeScheduleDto MapToDto(EmployeeSchedule entity)
        {
            if (entity == null)
            {
                return null!;
            }

            return new EmployeeScheduleDto
            {
                EmployeeId = entity.EmployeeId,
                EmployeeScheduleId = entity.EmployeeScheduleId,
                LimitInMinutes = entity.LimitInMinutes,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                EmployementType = entity.EmployementType,
            };
        }

        private EmployeeScheduleHistory MapToHistory(EmployeeSchedule entity)
        {
            if (entity == null)
            {
                return null!;
            }

            return new EmployeeScheduleHistory
            {
                EmployeeId = entity.EmployeeId,
                LimitInMinutes = entity.LimitInMinutes,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                EmployementType = entity.EmployementType,
                ValidFrom = entity.ModifiedAt,
                ValidTo = DateTime.Now
            };
        }

        public async Task<ICollection<EmployeeDto>> GetAllActiveEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllActiveEmployeesAsync();
            var activeEmployees = new List<EmployeeDto>();

            activeEmployees = employees.Select(MapToDto).ToList();

            return activeEmployees;
        }
    }
}
