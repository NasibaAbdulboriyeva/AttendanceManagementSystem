using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagementSystem.Infrastructure.Persistence.Repositories
{
    public class AttendanceLogRepository : IAttendanceLogRepository
    {
        private readonly AppDbContext _context;
        public AttendanceLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddLogsAsync(ICollection<AttendanceLog> logs)
        {
            if (logs == null || logs.Count == 0)
            {
                return;
            }
            _context.AttendanceLogs.AddRange(logs);
            await _context.SaveChangesAsync();
        }

        public async Task<ICollection<AttendanceLog>> GetLogsByEmployeeAndMonthAsync(long employeeId, DateTime month)
        {
            // 1. Oyning boshlanish va tugash sanalarini hisoblash
            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1); 

            var logs = await _context.AttendanceLogs
                .Where(log =>
                    log.EmployeeId == employeeId &&
                    log.RecordedTime.Date >= startDate.Date &&
                    log.RecordedTime.Date < endDate.Date     
                )
                .OrderBy(log => log.RecordedTime) 
                .AsNoTracking()
                .ToListAsync();

            return logs;
        }
        public async Task<ICollection<AttendanceLog>> GetLogsByEmployeeICCodeAsync(string code, DateTime startDate, DateTime endDate)
        {
            return await _context.AttendanceLogs
               .AsNoTracking()
               .Where(log => /*log.Employee.Code == code &&*/
                             log.RecordedTime.Date >= startDate.Date &&
                             log.RecordedTime.Date <= endDate.Date)
               .OrderBy(log => log.RecordedTime)
               .ToListAsync();
        }
        public async Task<ICollection<AttendanceLog>> GetLogsForAllEmployeesByDayAsync(DateTime targetDate)
        {
            return await _context.AttendanceLogs
                .AsNoTracking()
                .Where(log => log.RecordedTime.Date == targetDate.Date)
                .OrderBy(log => log.RecordedTime)
                .ToListAsync();
        }
        public async Task<ICollection<AttendanceLog>> GetLogsForEmployeeAndPeriodAsync(
            long employeeId,
            DateTime startDate,
            DateTime endDate)
        {

            var query = _context.AttendanceLogs

                .Where(log => log.EmployeeId == employeeId)

                .Where(log => log.RecordedTime.Date >= startDate.Date &&
                              log.RecordedTime.Date <= endDate.Date)
                .OrderBy(log => log.RecordedTime);

            return await query.ToListAsync();
        }
        public async Task<DateTime?> GetLastRecordedTimeAsync()
        {
            if (!await _context.AttendanceLogs.AnyAsync())
            {
                return null;
            }

            var lastTime = await _context.AttendanceLogs
                .AsNoTracking()
                .MaxAsync(log => log.RecordedTime);


            return lastTime;
        }
    }
}

