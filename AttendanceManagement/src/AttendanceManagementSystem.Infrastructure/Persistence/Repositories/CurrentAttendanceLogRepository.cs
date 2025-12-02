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
       
        public async Task<ICollection<CurrentAttendanceLog>>CreateLogAsync(ICollection<CurrentAttendanceLog> logs)
        {
            if (logs == null || !logs.Any())
            {
                return null;
            }

            await _context.CurrentAttendanceLogs.AddRangeAsync(logs);
            await _context.SaveChangesAsync();

            return logs;
        }
        public async Task DeleteMonthlyLogsAsync(long employeeId, DateTime month)
        {
            var startDate = new DateTime(month.Year, 11, 1);

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
        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}