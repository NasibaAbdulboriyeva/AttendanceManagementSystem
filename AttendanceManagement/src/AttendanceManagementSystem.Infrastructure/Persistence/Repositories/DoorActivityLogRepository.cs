
using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagementSystem.Infrastructure.Persistence.Repositories
{
    public class DoorActivityLogRepository : IDoorActivityLogRepository
    {
        private readonly AppDbContext _context;
        public DoorActivityLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddDoorLogsAsync(ICollection<DoorActivityLog> logs)
        {
            if (logs == null || logs.Count == 0)
            {
                return;
            }

            _context.DoorActivityLogs.AddRange(logs);
            await _context.SaveChangesAsync();
        }

        public async Task<ICollection<DoorActivityLog>> GetLogsByDayAsync(DateTime targetDate)
        {
            return await _context.DoorActivityLogs
                   .AsNoTracking()
                   .Where(log => log.RecordedTime.Date == targetDate.Date)
                   .OrderBy(log => log.RecordedTime)
                   .ToListAsync();
        }
    }
}
