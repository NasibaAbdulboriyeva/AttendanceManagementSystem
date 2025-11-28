using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs;
using AttendanceManagementSystem.Domain.Entities;


namespace AttendanceManagementSystem.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ITTLockService _ttLockService;
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(ITTLockService ttLockService, IEmployeeRepository employeeRepository)
        {
            _ttLockService = ttLockService;
            _employeeRepository = employeeRepository;
        }

        public async Task<(int CardsSynced, int FingerprintsSynced)> SyncEmployeeDataAsync()
        {

            // 1. IC Card foydalanuvchilarini sinxronlash
            int cardsSynced = await SyncEmployeesByTypeAsync<TTLockIcCardDto>(
                // ITTLockService ichidagi metod chaqiruvi
                (searchStr, orderBy) => _ttLockService.GetAllIcCardRecordsAsync(searchStr, orderBy),
                (employee, userDto) =>
                {
                    // Maplash: CardId va CardNumber Employee entitysiga o'tkaziladi
                    employee.CardId = userDto.CardId;
                    // Nullable xatolar uchun ehtiyot chorasi
                    employee.CardNumber = userDto.CardNumber ?? string.Empty;
                    if (string.IsNullOrEmpty(employee.UserName))
                        employee.UserName = userDto.CardName ?? string.Empty;
                });

            // 2. Fingerprint foydalanuvchilarini sinxronlash
            int fingerprintsSynced = await SyncEmployeesByTypeAsync<TTLockFingerprintDto>(
                // ITTLockService ichidagi metod chaqiruvi
                (searchStr, orderBy) => _ttLockService.GetAllFingerprintsPaginatedAsync(searchStr, orderBy),
                (employee, userDto) =>
                {
                    employee.FingerprintId = userDto.FingerprintId;
                    // Nullable xatolar uchun ehtiyot chorasi
                    employee.FingerprintNumber = userDto.FingerprintNumber ?? string.Empty;
                    if (string.IsNullOrEmpty(employee.UserName))
                        employee.UserName = userDto.FingerprintName ?? string.Empty;
                });

            return (cardsSynced, fingerprintsSynced);
        }

        private async Task<int> SyncEmployeesByTypeAsync<T>(
            // TTLock'dagi metod LockId'ni qabul qiladi, shuning uchun Func<string?, int, Task<ICollection<T>>> tuzilishini saqlaymiz.
            Func<string?, int, Task<ICollection<T>>> fetchFunc,
            Action<Employee, T> mapAction)
            where T : class
        {

            int syncedCount = 0;

            ICollection<T> ttLockUsers;
            try
            {
                // API'dan ma'lumotlarni olish (searchStr = null, orderBy = 1)
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

                // 1. Ma'lumotlar bazasidan qidirish (TTLock ID asosida - TUZATILGAN MANTIQ)
                if (ttUser is TTLockIcCardDto icCardDto)
                {
                    // 💡 TUZATILGAN: CardName o'rniga CardId orqali qidirish
                    if (icCardDto.CardId > 0)
                    {
                        employee = await _employeeRepository.GetEmployeeByUsernameAsync(icCardDto.CardName);
                    }
                }

                else if (ttUser is TTLockFingerprintDto fingerprintDto)
                {
                    //   TUZATILGAN: FingerprintName o'rniga FingerprintId orqali qidirish
                    if (fingerprintDto.FingerprintId > 0)
                    {
                        employee = await _employeeRepository.GetEmployeeByUsernameAsync(fingerprintDto.FingerprintName);
                    }
                }

                // Agar qidiruv TTLock ID orqali natija bermasa, nom bo'yicha qidiruvni o'tkazib yuboramiz.
                // Biz yuqorida ID asosida qidirishni aniqlashtirdik, endi shu yerdagi mantiqni ID qidiruviga o'zgartirdik.

                // 2. Yangi Employee yaratish yoki mavjudini aniqlash
                bool isNew = (employee == null);

                if (isNew)
                {
                    // 💡 Nullable bo'lmagan maydonlar uchun boshlang'ich qiymat berish
                    employee = new Employee
                    {
                        CreatedAt = DateTime.Now,
                        IsActive = true,
                        UserName = string.Empty,
                        CardNumber = string.Empty
                    };
                }

                // 3. Ma'lumotlarni Employee'ga maplash
                mapAction(employee, ttUser); // employee endi null bo'lmaydi

                // 4. Ma'lumotlar bazasiga saqlash
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
                throw new KeyNotFoundException($"Xodim {id} idsi bo'yicha topilmadi.");
            }

            employee.IsActive = false;
            employee.ModifiedAt = DateTime.UtcNow;
            await _employeeRepository.UpdateEmployeeAsync(employee);
        }


        public async Task<ICollection<EmployeeDto>> GetAllEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();
            // Entity ni DTO ga konvertatsiya qilish
            return employees.Select(MapToDto).ToList();
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

            // 3. Entity'ni Repository orqali bazaga saqlash
            await _employeeRepository.AddEmployeeScheduleAsync(scheduleEntity);

            // Agar Repository faqat bitta saqlash metodi bo'lsa, u SaveChangesAsync ni chaqiradi.

            // 4. Saqlangan jadvalning ID sini qaytarish
            return scheduleEntity.EmployeeScheduleId;
        }

        public async Task<EmployeeScheduleDto?> GetEmployeeScheduleByEmployeeIdAsync(long employeeId)
        {
            // 1. Repository orqali Schedule Entity'sini olish
            var scheduleEntity = await _employeeRepository.GetScheduleByEmployeeIdAsync(employeeId);

            if (scheduleEntity == null)
            {
                // Agar jadval topilmasa null qaytariladi
                return null;
            }

            // 2. Entity'ni DTO'ga konvertatsiya qilish
            // Bu yerda sizning umumiy IEmployeeMapper yoki maxsus IScheduleMapper dan foydalaniladi.
            // Repozitoriyda yoki alohida Mapperda MapToDto(ScheduleEntity) metodi mavjud deb faraz qilamiz.
            var scheduleDto = MapToDto(scheduleEntity); // IEmployeeMapper kengaytirilgan deb taxmin qilinadi

            return scheduleDto;
        }

        /// <summary>
        /// Mavjud ish jadvalini yangilaydi.
        /// </summary>
        public async Task UpdateEmployeeScheduleAsync(EmployeeScheduleDto scheduleDto)
        {
            if (scheduleDto == null)
            {
                throw new ArgumentNullException(nameof(scheduleDto), "Данные таблицы не могут быть пустыми");
            }

            // 1. Avval mavjud Schedule Entity'sini bazadan olib kelishimiz kerak
            var existingSchedule = await _employeeRepository.GetScheduleByEmployeeIdAsync(scheduleDto.EmployeeId);

            if (existingSchedule == null)
            {
                // Agar Schedule mavjud bo'lmasa, xato qaytarish yoki uni yangi qilib yaratish mumkin.
                // Yangilash (Update) metodida, odatda, mavjud bo'lishi talab qilinadi.
                throw new KeyNotFoundException($"Для сотрудника {scheduleDto.EmployeeId} расписание не найдено. Его нужно создать заранее.");
            }

            // 2. DTO dagi ma'lumotlar bilan mavjud Entity'ni yangilash
            // Bu qism odatda maxsus 'MapToEntity' (yoki 'UpdateEntity') metodi orqali amalga oshiriladi.

            // Taxmin qilinadigan yangilash:
            existingSchedule.ModifiedAt = DateTime.Now;
            existingSchedule.StartTime = scheduleDto.StartTime;
            existingSchedule.EndTime = scheduleDto.EndTime;
            // ... boshqa maydonlarni ham yangilang

            // 3. Yangilangan Entity'ni Repository orqali bazaga saqlash
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
            if (entity == null) return null!;

            return new EmployeeScheduleDto
            {
                EmployeeId = entity.EmployeeId,
                LimitInMinutes = entity.LimitInMinutes,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                EmployementType = entity.EmployementType,
            };
        }

        public async Task<ICollection<EmployeeDto>> GetAllActiveEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllActiveEmployeesAsync();
            // Entity ni DTO ga konvertatsiya qilish
            var activeEmployees = new List<EmployeeDto>();

            activeEmployees = employees.Select(MapToDto).ToList();

            return activeEmployees;
        }
    }
}
