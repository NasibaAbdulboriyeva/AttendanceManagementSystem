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
    }
}