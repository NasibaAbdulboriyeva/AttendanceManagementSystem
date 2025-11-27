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

        public AttendanceLogService(ITTLockService ttLockService, IAttendanceLogRepository logRepository, IEmployeeRepository employeeRepository, IOptions<TTLockSettings> settings)
        {
            _ttLockService = ttLockService;
            _logRepository = logRepository;
            _employeeRepository = employeeRepository;
            _settings = settings.Value;
        }

        public async Task<int> SyncAttendanceLogsAsync()
        {
            var lockId = _settings.LockId;

            DateTime? lastRecordTime = await _logRepository.GetLastRecordedTimeAsync();

            DateTimeOffset syncStart;

            if (lastRecordTime.HasValue)
            {
                DateTimeOffset lastRecordOffset = new DateTimeOffset(lastRecordTime.Value, TimeSpan.Zero);
                syncStart = lastRecordOffset.AddSeconds(2);
            }
            else
            {
                //bu agar bazada umuman log bomasa yani 1 chi yaratvolishchun kere 
                // 30 kun oldingi hozgri kundan boshlab (naprimer)
                syncStart = new DateTimeOffset(DateTime.UtcNow.AddDays(-30).Date, TimeSpan.Zero);
            }

            DateTimeOffset syncEnd = DateTimeOffset.UtcNow;

            if (syncStart >= syncEnd)
            {
                return 0;
            }

            long startTimestampMs = syncStart.ToUnixTimeMilliseconds();
            long endTimestampMs = syncEnd.ToUnixTimeMilliseconds();

            var ttLockRecords = await _ttLockService.GetAllAttendanceLockRecordsAsync(startDate: startTimestampMs,
                endDate: endTimestampMs);

            if (ttLockRecords == null || !ttLockRecords.Any())
            {
                return 0;
            }

            var allUsernames = ttLockRecords
                .Select(r => r.Username)
                .Where(u => !string.IsNullOrEmpty(u))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            IReadOnlyDictionary<string, Employee> employeeLookup =
            await _employeeRepository.GetEmployeesByUsernamesAsync(allUsernames);
            var sortedTtLockRecords = ttLockRecords
            .OrderBy(ttRecord => ttRecord.LockDate)
            .ToList();
            var logsToSave = new List<AttendanceLog>();

            foreach (var recordDto in sortedTtLockRecords)
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

            AttendanceStatus status = dto.Success == 1
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


    }

}
