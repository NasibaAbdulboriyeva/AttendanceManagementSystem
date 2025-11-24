using AttendanceManagementSystem.Api.Configurations;
using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs;
using AttendanceManagementSystem.Domain.Entities;
using Microsoft.Extensions.Options;

namespace AttendanceManagementSystem.Application.Services
{
    public class AttendanceLogService : IAttendanceLogService
    {
        private readonly TTLockSettings _settings;
        private readonly ITTLockService _ttLockService;
        private readonly IAttendanceLogRepository _logRepository;
        private readonly IEmployeeRepository _employeeRepository;

    public AttendanceLogService(
        ITTLockService ttLockService,
        IAttendanceLogRepository logRepository,
        IEmployeeRepository employeeRepository, IOptions<TTLockSettings> settings)
        {
            _ttLockService = ttLockService;
            _logRepository = logRepository;
            _employeeRepository = employeeRepository;
            _settings = settings.Value;
        }

        public async Task<int> SyncAttendanceLogsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate )
        {
            var lockId = _settings.LockId;

            DateTimeOffset syncStart = startDate ?? await GetLastSyncTimeAsync(lockId);

            DateTimeOffset syncEnd = endDate ?? DateTimeOffset.UtcNow;

            long startTimestampMs = syncStart.ToUnixTimeMilliseconds();
            long endTimestampMs = syncEnd.ToUnixTimeMilliseconds();

            var ttLockRecords = await _ttLockService.GetAllAttendanceLockRecordsAsync( startDate: startTimestampMs,endDate: endTimestampMs);

            if (ttLockRecords == null || !ttLockRecords.Any())
            {
                return 0;

            }

            // 1. Barcha foydalanuvchilarni lug'atga olish
            var allUsernames = ttLockRecords
                .Select(r => r.Username)
                .Where(u => !string.IsNullOrEmpty(u))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            IReadOnlyDictionary<string, Employee> employeeLookup =
                await _employeeRepository.GetEmployeesByUsernamesAsync(allUsernames);

            var logsToSave = new List<AttendanceLog>();

            foreach (var recordDto in ttLockRecords)
            {
                var logEntity = MapToAttendanceLog(recordDto, employeeLookup);
                if (logEntity != null)
                {
                    logsToSave.Add(logEntity);
                }
            }

            if (logsToSave.Any())
            {
                await _logRepository.AddLogsAsync(logsToSave);
            }

            return logsToSave.Count;
        }

        private AttendanceLog? MapToAttendanceLog(TTLockRecordDto dto, IReadOnlyDictionary<string, Employee> employeeLookup)
        {
            DateTimeOffset logTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(dto.LockDate);

            AttendanceStatus status = dto.Success ==1
                ? AttendanceStatus.Success
                : AttendanceStatus.Unknown;

            if (string.IsNullOrEmpty(dto.Username) || !employeeLookup.TryGetValue(dto.Username, out var employee))
            {
                return null;

            }

            return new AttendanceLog
            {
                EmployeeId = employee.EmployeeId,
                RecordId = dto.RecordId,
                RecordedTime = logTimeOffset.LocalDateTime,
                Status = status,
                RawUsername = dto.Username,
                CreatedAt = DateTime.UtcNow
               
            };
        }

        public Task<ICollection<AttendanceLog>> GetLogsByEmployeeIdAsync(long employeeId, DateTime startDate, DateTime endDate)
        {
            var attendanceLog = _logRepository.GetLogsForEmployeeAndPeriodAsync(employeeId, startDate, endDate);
            if (attendanceLog == null)
            {
                throw new Exception("No attendanceLogs found for the specified employee and date range.");
            }
            return attendanceLog;
        }

        public Task<ICollection<AttendanceLog>> GetLogsForAllEmployeesByDayAsync(DateTime targetDate)
        {
            var attendanceLog = _logRepository.GetLogsForAllEmployeesByDayAsync(targetDate);
            if (attendanceLog == null)
            {
                throw new Exception("No attendanceLogs found for the specified date.");

            }
            return attendanceLog;
        }

        private Task<DateTimeOffset> GetLastSyncTimeAsync(string lockId)
        {
            // Bu yerga oxirgi muvaffaqiyatli sinxronizatsiya vaqtini saqlash mexanizmi qo‘yilishi mumkin
            return Task.FromResult(DateTimeOffset.UtcNow.AddDays(-31));
        }
    }

}
