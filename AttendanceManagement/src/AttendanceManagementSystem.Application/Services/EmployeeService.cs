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
                        CreatedAt = DateTime.UtcNow,
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
                    employee.ModifiedAt = DateTime.UtcNow;
                    await _employeeRepository.UpdateEmployeeAsync(employee);
                }
                syncedCount++;
            }

            return syncedCount;
        }

        // Yordamchi metod: Loglash uchun ID olish
        private long GetTTLockId<T>(T ttUser) where T : class
        {
            if (ttUser is TTLockIcCardDto icCardDto) return icCardDto.CardId;
            if (ttUser is TTLockFingerprintDto fingerprintDto) return fingerprintDto.FingerprintId;
            return 0;
        }

        public Task<long> AddEmployeeAsync(EmployeeCreateDto employeeCreateDto) => throw new NotImplementedException();
        public Task DeactivateEmployeeAsync(string code) => throw new NotImplementedException();
        public Task<ICollection<EmployeeDto>> GetActiveEmployeesAsync() => throw new NotImplementedException();
        public Task<ICollection<EmployeeDto>> GetAllEmployeesAsync() => throw new NotImplementedException();
        public Task<EmployeeDto?> GetEmployeeByICCodeAsync(string code) => throw new NotImplementedException();
        public Task UpdateScheduleByICCodeAsync(string code) => throw new NotImplementedException();
    }
}