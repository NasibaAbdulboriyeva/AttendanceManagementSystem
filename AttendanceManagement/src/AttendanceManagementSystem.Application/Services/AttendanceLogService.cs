using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs;
using AttendanceManagementSystem.Domain.Entities;


namespace AttendanceManagementSystem.Application.Services
{
    public class AttendanceLogService : IAttendanceLogService
    {
        private readonly ITTLockService _ttLockService;
        private readonly IAttendanceLogRepository _logRepository;

        public AttendanceLogService(
            ITTLockService ttLockService,
            IAttendanceLogRepository logRepository
           )
        {
            _ttLockService = ttLockService;
            _logRepository = logRepository;
        }

        public async Task<int> SyncAttendanceLogsAsync(int lockId, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {

            DateTimeOffset lastSyncTime = await GetLastSyncTimeAsync(lockId);
            long startTimestampMs = lastSyncTime.ToUnixTimeMilliseconds();
            long endTimestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            try
            {
                var ttLockRecords = await _ttLockService.GetAllAttendanceLockRecordsAsync(
                    lockId: lockId,
                    startDate: startTimestampMs,
                    endDate: endTimestampMs
                );

                if (ttLockRecords == null || !ttLockRecords.Any())
                {
                    return 0;
                }


                var logsToSave = new List<AttendanceLog>();

                foreach (var recordDto in ttLockRecords)
                {

                    DateTimeOffset logTime = ParseTimestampToDateTimeOffset(recordDto.LockDate);
                    var logEntity = MapToAttendanceLog(recordDto);
                    if (logEntity.Status == AttendanceStatus.Success)
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
            catch (Exception ex)
            {
                throw;
            }
        }

        public Task<ICollection<AttendanceLog>> GetLogsByEmployeeIdAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            return _logRepository.GetLogsByEmployeeIdAsync(employeeId, startDate, endDate);
        }

        public Task<ICollection<AttendanceLog>> GetLogsForAllEmployeesByDayAsync(DateTime targetDate)
        {
            return _logRepository.GetLogsForAllEmployeesByDayAsync(targetDate);
        }

        public Task<ICollection<AttendanceLog>> GetLogsForEmployeeAndPeriodAsync(long employeeId, DateTime startDate, DateTime endDate)
        {
            return _logRepository.GetLogsByEmployeeIdAsync(employeeId, startDate, endDate);
        }


        private DateTimeOffset ParseTimestampToDateTimeOffset(long milliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
        }

        private Task<DateTimeOffset> GetLastSyncTimeAsync(int lockId)
        {

            return Task.FromResult(DateTimeOffset.UtcNow.AddDays(-30));
        }

        private AttendanceLog MapToAttendanceLog(TTLockRecordDto dto)
        {

            DateTimeOffset logTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(dto.LockDate);

            AttendanceStatus status = dto.Success == 1
                                        ? AttendanceStatus.Success
                                        : AttendanceStatus.Unknown;

            long mappedEmployeeId = 0;

            return new AttendanceLog
            {
                EmployeeId = mappedEmployeeId,
                RecordedTime = logTimeOffset.UtcDateTime,
                Status = status,
                RawUsername = dto.Username ?? dto.KeyboardPwd ?? dto.RecordId.ToString(),
                CreatedAt = DateTime.UtcNow

            };
        }
    }
}