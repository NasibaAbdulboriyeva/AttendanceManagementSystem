using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagementSystem.Infrastructure.Persistence.Repositories
{
    public class CurrentAttendanceLogRepository : ICurrentAttendanceLogRepository
    {
        private readonly AppDbContext _context;

        public CurrentAttendanceLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<CurrentAttendanceLog>> CreateLogAsync(ICollection<CurrentAttendanceLog> logs)
        {
            if (logs == null || !logs.Any())
            {
                return null;
            }

            await _context.CurrentAttendanceLogs.AddRangeAsync(logs);
            await _context.SaveChangesAsync();

            return logs;
        }

        public async Task<ICollection<CurrentAttendanceLog>> GetLogsWithoutEntryTimeAsync(long employeeId, DateOnly month)
        {
            var logs = await _context.CurrentAttendanceLogs
                .Where(l => l.EmployeeId == employeeId &&
                            l.EntryDay.Year == month.Year &&
                            l.EntryDay.Month == month.Month &&
                            l.FirstEntryTime == default) 
                .ToListAsync();

            return logs;
        }
        public async Task<bool> HasMonthlyAttendanceLogs(DateTime month)
        {
            // Oyning boshlanishi va oxirini aniqlash
            var startOfMonth = new DateTime(month.Year, month.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

           
            bool exists = await _context.CurrentAttendanceLogs 
                .AnyAsync(log => log.CreatedAt >= startOfMonth && log.CreatedAt <= endOfMonth);
            return exists;
        }
        public async Task<DateTime?> GetLastAttendanceLogDateAsync(DateTime targetMonth)
        {
            var targetYear = targetMonth.Year;
            var targetMonthValue = 11;

            var lastCreatedDate = await _context.CurrentAttendanceLogs
                .Where(log => log.EntryDay.Year == targetYear && log.EntryDay.Month == targetMonthValue)
                .MaxAsync(log => (DateTime?)log.CreatedAt); 

            return lastCreatedDate;
        }


        public async Task UpdateRangeAsync(IEnumerable<CurrentAttendanceLog> logs)
        {
            if (logs == null || !logs.Any())
            {
                return;
            }

            _context.CurrentAttendanceLogs.UpdateRange(logs);

           
            await _context.SaveChangesAsync();
        }
        public async Task DeleteMonthlyLogsAsync(long employeeId, DateTime month)
        {
            var startDate = new DateTime(month.Year, 12, 1);

            var logsToDelete = await _context.CurrentAttendanceLogs
                .Where(l => l.EmployeeId == employeeId &&
                l.EntryDay.Year == startDate.Year &&
                l.EntryDay.Month == startDate.Month)
            .ToListAsync();

            _context.CurrentAttendanceLogs.RemoveRange(logsToDelete);
            await _context.SaveChangesAsync();
        }

        public async Task<CurrentAttendanceLog> GetLogByIdAsync(long id)
        {
            var log = await _context.CurrentAttendanceLogs
                                 .FirstOrDefaultAsync(log => log.CurrentAttendanceLogId == id);
            return log;
        }

        public async Task<ICollection<CurrentAttendanceLog>> GetAllLogsAsync()
        {
            return await _context.CurrentAttendanceLogs
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        public async Task<ICollection<CurrentAttendanceLog>> GetLogsByEmployeeIdAsync(long employeeId)
        {
            return await _context.CurrentAttendanceLogs
                                 .AsNoTracking()
                                 .Where(log => log.EmployeeId == employeeId)
                                 .OrderBy(log => log.EntryDay) 
                                 .ToListAsync();
        }

        public async Task<CurrentAttendanceLog> UpdateLogAsync(CurrentAttendanceLog log)
        {
            _context.CurrentAttendanceLogs.Update(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task DeleteLogAsync(long id)
        {
            var log = await _context.CurrentAttendanceLogs.FindAsync(id);
            if (log != null)
            {
                _context.CurrentAttendanceLogs.Remove(log);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<CurrentAttendanceLog> GetLogByEmployeeIdAndEntryDayAsync(long employeeId, DateOnly entryDay)
        {
            var targetDate = entryDay;

            var log = await _context.CurrentAttendanceLogs
                .FirstOrDefaultAsync(l =>
                    l.EmployeeId == employeeId &&
                    l.EntryDay == targetDate);

            return log;
        }
        public async Task<int> GetLateArrivalsForPeriodAsync(long employeeId, DateTime month)
        {
            var dateTimeStartDate = new DateTime(month.Year, month.Month, 1);
            var dateTimeEndDate = dateTimeStartDate.AddMonths(1);

            var startDate = DateOnly.FromDateTime(dateTimeStartDate);
            var endDate = DateOnly.FromDateTime(dateTimeEndDate);

            var totalUnjustifiedLateMinutes = await _context.CurrentAttendanceLogs 
                .Where(log => log.EmployeeId == employeeId &&
                           
                              log.EntryDay >= startDate &&
                              log.EntryDay < endDate &&
                              log.IsWorkingDay == true &&
                              log.IsJustified == false)
                .SumAsync(log => log.LateArrivalMinutes);

            return totalUnjustifiedLateMinutes;
        }
        public async Task<int> GetLateArrivalsForDayAsync(long employeeId, DateOnly day)
        {
            var logs = await _context.CurrentAttendanceLogs
                .Where(log => log.EmployeeId == employeeId && log.EntryDay == day)
                .ToListAsync();

            if (!logs.Any() || logs.All(l => l.FirstEntryTime== default(TimeOnly)))
            {
                return -1; 
            }

            var totalUnjustifiedLateMinutes = logs
                .Where(log => log.IsWorkingDay == true && log.IsJustified == false)
                .Sum(log => log.LateArrivalMinutes);

            return totalUnjustifiedLateMinutes;
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}