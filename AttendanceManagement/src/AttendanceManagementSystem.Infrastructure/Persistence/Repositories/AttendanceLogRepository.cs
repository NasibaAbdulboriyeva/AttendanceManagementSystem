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

        public async Task<ICollection<AttendanceLog>> GetLogsByEmployeeIdAsync(long employeeId, DateTime startDate, DateTime endDate)
        {
            return await _context.AttendanceLogs
               .AsNoTracking()
               .Where(log => log.EmployeeId == employeeId &&
                             log.RecordedTime.Date >= startDate.Date &&
                             log.RecordedTime.Date <= endDate.Date)
               .OrderBy(log=>log.RecordedTime)
               .ToListAsync();
        }
        public async Task<ICollection<AttendanceLog>> GetLogsByEmployeeICCodeAsync(string code, DateTime startDate, DateTime endDate)
        {
            return await _context.AttendanceLogs
               .AsNoTracking()
               .Where(log => log.Employee.Code == code &&
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
    }
}

