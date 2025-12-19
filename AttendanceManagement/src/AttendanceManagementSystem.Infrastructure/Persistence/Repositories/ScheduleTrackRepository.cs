using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagementSystem.Infrastructure.Persistence.Repositories
{
    public class ScheduleTrackRepository : IScheduleTrackRepository
    {
        private readonly AppDbContext _context;
        public ScheduleTrackRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<long> AddScheduleHistoryAsync(EmployeeScheduleHistory scheduleHistory)
        {
            await _context.EmployeeScheduleHistories.AddAsync(scheduleHistory);
            await _context.SaveChangesAsync();
            return scheduleHistory.EmployeeScheduleHistoryId;
        }

        public async Task<EmployeeScheduleHistory> GetScheduleByDateAndByEmployeeIdAsync(long employeeId,DateTime targetDate)
        {
            var histories = await _context.EmployeeScheduleHistories
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == employeeId &&
                    x.ValidFrom.Date == targetDate.Date);
            return histories;//null bo'sa eski ni obkelishi kere yani schedule digi vaqtini 
        }

        public async Task<ICollection<EmployeeScheduleHistory>> GetScheduleHistoryByEmployeeIdAsync(long employeeId)
        {
            return await _context.EmployeeScheduleHistories
                .AsNoTracking()
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.ValidFrom)
                .ToListAsync();
        }
    }

}
