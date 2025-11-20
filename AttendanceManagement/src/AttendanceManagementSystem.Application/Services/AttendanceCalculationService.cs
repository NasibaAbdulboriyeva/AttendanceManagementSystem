using AttendanceManagementSystem.Application.Abstractions; // Interfeys bu yerda
using AttendanceManagementSystem.Application.DTOs;
using AttendanceManagementSystem.Domain.Entities;


namespace AttendanceManagementSystem.Application.Services
{
    // Konstanta: Ruxsat berilgan kechikish (Admin uni DB'dan o'qishi kerak, hozircha konstant)
    public static class SystemSettings
    {
        public const int LateThresholdMinutes = 5;
        public const int MonthlyLateLimitMinutes = 80; // Oylik ruxsat berilgan limit
    }

    public class AttendanceCalculationService : IAttendanceCalculationService
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IAttendanceLogRepository _logRepo;
        private readonly IEmployeeSummaryRepository _employeeSummaryRepo;
        // private readonly ISystemConfigService _configService; // Agar limit DBda saqlansa

        public AttendanceCalculationService(
            IEmployeeRepository employeeRepo,
            IAttendanceLogRepository logRepo,
            IEmployeeSummaryRepository employeeSummaryRepo)
        {
            _employeeRepo = employeeRepo;
            _logRepo = logRepo;
            _employeeSummaryRepo = employeeSummaryRepo;
        }

        public async Task<ICollection<CalculatedDailyLogDto>> GetCalculatedAttendanceForPeriodAsync(
           string employeeCode,
            DateTime startDate,
            DateTime endDate)
        {
            var employee = await _employeeRepo.GetEmployeeByICCodeAsync(employeeCode);
            if (employee == null)
            {
                return new List<CalculatedDailyLogDto>();
            }

            var schedule = await _employeeRepo.GetScheduleByEmployeeIdAsync(employee.EmployeeId);
            if (schedule == null)
            {
                return new List<CalculatedDailyLogDto>();
            }

            var allLogs = await _logRepo.GetLogsForEmployeeAndPeriodAsync(
                employee.EmployeeId, startDate.Date, endDate.Date);

            var firstCheckInLogs = allLogs
                .Where(log => log.Status == AttendanceStatus.Success)
                .GroupBy(log => log.RecordedTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    FirstCheckInTime = g.OrderBy(log => log.RecordedTime).First().RecordedTime
                })
                .ToList();

            var results = new List<CalculatedDailyLogDto>();
            var lateThreshold = SystemSettings.LateThresholdMinutes;

            foreach (var record in firstCheckInLogs)
            {
                DateTime scheduledStart = record.Date.Add(schedule.StartTime);
                DateTime actualCheckIn = record.FirstCheckInTime;

                TimeSpan lateDuration = actualCheckIn - scheduledStart;
                int totalLateMinutes = (int)Math.Ceiling(lateDuration.TotalMinutes); // Yuqoriga yaxlitlash

                int lateBeyondLimit = 0;
                if (totalLateMinutes > lateThreshold)
                {
                    lateBeyondLimit = totalLateMinutes - lateThreshold;
                }

                results.Add(new CalculatedDailyLogDto
                {
                    Date = record.Date,
                    EmployeeCode = employeeCode,
                    EmployeeFullName = employee.FullName,
                    ScheduledStartTime = schedule.StartTime,
                    FirstCheckInTime = actualCheckIn,
                    LateMinutesTotal = totalLateMinutes > 0 ? totalLateMinutes : 0,
                    LateMinutesBeyondLimit = lateBeyondLimit,
                });
            }

                return results;
            
        }


        public async Task<ICollection<CalculatedDailyLogDto>> GetLateArrivalsForPeriodAsync(
            string employeeCode,
            DateTime startDate,
            DateTime endDate)
        {
            var allCalculatedLogs = await GetCalculatedAttendanceForPeriodAsync(
                employeeCode, startDate, endDate);

            return allCalculatedLogs
                .Where(log => log.LateMinutesBeyondLimit > 0)
                .ToList();
        }

       
        public Task<EmployeeSummaryDto> CalculateAndSaveMonthlySummaryAsync(string employeeCode, int year, int month)
        {
            
            throw new NotImplementedException();
        }

        public Task<RemainingLimitDto> CheckRemainingLateLimitAsync(string employeeCode, int year, int month)
        {
            throw new NotImplementedException();
        }
    }
}