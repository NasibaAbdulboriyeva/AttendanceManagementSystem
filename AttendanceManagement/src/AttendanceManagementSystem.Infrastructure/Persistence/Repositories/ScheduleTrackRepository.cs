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

        public async Task<ICollection<EmployeeScheduleHistory>> GetScheduleByDateAndByEmployeeIdAsync(long employeeId, DateOnly targetDate)
        {
            
            var histories = await _context.EmployeeScheduleHistories
                .AsNoTracking()
                .Where(x => x.EmployeeId == employeeId &&
                            x.ValidFrom.Year == targetDate.Year &&
                            x.ValidFrom.Month == targetDate.Month)
                .OrderBy(x => x.ValidFrom) 
                .ToListAsync();

            return histories;
        }


    }

}
