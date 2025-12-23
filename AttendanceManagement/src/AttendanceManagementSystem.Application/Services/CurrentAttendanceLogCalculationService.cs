using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs;
using AttendanceManagementSystem.Domain.Entities;
namespace AttendanceManagementSystem.Application.Services;

public class CurrentAttendanceLogCalculationService : ICurrentAttendanceLogCalculationService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAttendanceLogRepository _attendanceLogRepository;
    private readonly ICurrentAttendanceLogRepository _currentAttendanceLogRepository;
    private readonly IScheduleTrackRepository _trackRepository;

    public CurrentAttendanceLogCalculationService(IAttendanceLogRepository attendanceLogRepository, IEmployeeRepository employeeRepository, ICurrentAttendanceLogRepository currentAttendanceLogRepository, IScheduleTrackRepository trackRepository)
    {
        _attendanceLogRepository = attendanceLogRepository;
        _employeeRepository = employeeRepository;
        _currentAttendanceLogRepository = currentAttendanceLogRepository;
        _trackRepository = trackRepository;
    }

    public async Task<int> GetLateArrivalsForPeriodAsync(long employeeId, DateTime month)
    {
        return await _currentAttendanceLogRepository.GetLateArrivalsForPeriodAsync(employeeId, month);
    }

    public async Task<bool> HasMonthlyAttendanceLogs(DateTime month)
    {
        bool exists = await _currentAttendanceLogRepository.HasMonthlyAttendanceLogs(month);

        return exists;
    }

    public async Task<Dictionary<long, int>> GetEmployeesLateSummaryAsync(DateTime month)
    {
        var activeEmployeeIds = await _employeeRepository.GetAllActiveEmployeesAsync();
        var minutes = 0;
        var lateSummary = new Dictionary<long, int>();
        foreach (var employee in activeEmployeeIds)
        {
            minutes = await _currentAttendanceLogRepository.GetLateArrivalsForPeriodAsync(employee.EmployeeId, month);
            lateSummary.Add(employee.EmployeeId, minutes);
        }

        return lateSummary;
    }

    public async Task<Dictionary<long, int>> GetEmployeesDailyLateSummaryAsync(DateOnly day)
    {
        var activeEmployeeIds = await _employeeRepository.GetAllActiveEmployeesAsync();
        var minutes = 0;
        var lateSummary = new Dictionary<long, int>();
        foreach (var employee in activeEmployeeIds)
        {
            minutes = await _currentAttendanceLogRepository.GetLateArrivalsForDayAsync(employee.EmployeeId, day);
            lateSummary.Add(employee.EmployeeId, minutes);
        }

        return lateSummary;
    }
    public async Task<DateTime?> GetLastAttendanceLogDate(DateTime targetMonth)
    {
        try
        {
            return await _currentAttendanceLogRepository.GetLastAttendanceLogDateAsync(targetMonth);
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task ProcessAllEmployeesMonthlyAttendanceAsync(DateTime month)
    {
        var activeEmployeeIds = await _employeeRepository.GetAllActiveEmployeesAsync();

        foreach (var employee in activeEmployeeIds)
        {
            await GetAndSaveMonthlyAttendanceCalendarAsync(employee.EmployeeId, month);
        }

    }

    public async Task ProcessUpdateForAllEmployeesMonthlyAttendanceAsync(DateOnly month)
    {
        var activeEmployeeIds = await _employeeRepository.GetAllActiveEmployeesAsync();

        foreach (var employee in activeEmployeeIds)
        {
            await UpdateMonthlyEntryTimesAsync(employee.EmployeeId, month);
        }
    }

    public bool IsWorkingDay(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday ||date.DayOfWeek == DayOfWeek.Sunday)
        {
            return false;
        }

        return true;
    }

    public int CalculateLateMinutes(CurrentAttendanceCalendar calendarDto, DateTime targetDate)
    {
        if (!calendarDto.IsWorkingDay)
        {
            return 0;

        }

        if (calendarDto.IsJustified)
        {
            return 0;
        }

        if (calendarDto.FirstEntryTime == default || calendarDto.ScheduledStartTime == default)
        {
            return 0;
        }

        var scheduledStartDateTime = targetDate.Date.Add(calendarDto.ScheduledStartTime.ToTimeSpan());
        var actualEntryDateTime = targetDate.Date + calendarDto.FirstEntryTime.ToTimeSpan();

        if (actualEntryDateTime <= scheduledStartDateTime)
        {
            return 0;
        }

        var lateDuration = actualEntryDateTime - scheduledStartDateTime;
        calendarDto.CalculatedAt = DateTime.Now;
        calendarDto.ModifiedAt = DateTime.Now;

        return (int)lateDuration.TotalMinutes;
    }

    public async Task<ICollection<CurrentAttendanceCalendar>> GetAndSaveMonthlyAttendanceCalendarAsync(long employeeId, DateTime month)
    {
        var startDate = new DateTime(month.Year, month.Month, 1); // Oy statik bo'lmasligi kerak (12 ni month.Month ga o'zgartirdim)
        int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);

        var allExistingLogs = await _attendanceLogRepository.GetLogsByEmployeeAndMonthAsync(employeeId, month);

        // Tarixiy va joriy jadvallarni olish (Update metodidagidek)
        var scheduleHistory = await _trackRepository.GetScheduleByDateAndByEmployeeIdAsync(employeeId, DateOnly.FromDateTime(startDate));
        var currentSchedule = await _employeeRepository.GetScheduleByEmployeeIdAsync(employeeId);

        var allSchedules = (scheduleHistory ?? new List<EmployeeScheduleHistory>())
            .Select(s => new { s.StartTime, FullDateTime = s.ValidFrom })
            .Append(new { currentSchedule.StartTime, FullDateTime = currentSchedule.ModifiedAt })
            .OrderByDescending(s => s.FullDateTime)
            .ToList();

        var dailyLogsGrouped = allExistingLogs
            .GroupBy(log => log.RecordedTime.Date)
            .ToDictionary(g => g.Key, g => g.OrderBy(log => log.RecordedTime).First());

        var monthlyCalendar = new List<CurrentAttendanceCalendar>();

        for (int i = 0; i < daysInMonth; i++)
        {
            var targetDate = startDate.AddDays(i);
            var targetDayTime = targetDate; // Kunni boshlanishi

            // O'sha kunga mos jadvalni topish
            var applicableSchedule = allSchedules
                .FirstOrDefault(s => targetDayTime >= s.FullDateTime)
                ?? allSchedules.LastOrDefault();

            CurrentAttendanceCalendar calendarDto;

            if (dailyLogsGrouped.TryGetValue(targetDate.Date, out var firstEntryLog))
            {
                calendarDto = MapToCalendarDto(firstEntryLog, currentSchedule); 
                calendarDto.ScheduledStartTime = applicableSchedule?.StartTime ?? currentSchedule.StartTime;
            }
            else
            {
                calendarDto = CreateDefaultCalendarDto(employeeId, targetDate, currentSchedule);
                calendarDto.ScheduledStartTime = applicableSchedule?.StartTime ?? currentSchedule.StartTime;
            }

            calendarDto.CreatedAt = DateTime.Now;
            calendarDto.LateMinutesTotal = CalculateLateMinutes(calendarDto, targetDate);
            monthlyCalendar.Add(calendarDto);
        }

        var logsToInsert = monthlyCalendar.Select(dto => MapCalendarDtoToEntity(dto)).ToList();
        await _currentAttendanceLogRepository.CreateLogAsync(logsToInsert);

        return monthlyCalendar;
    }
    public async Task UpdateMonthlyEntryTimesAsync(long employeeId, DateOnly month)
    {
        
        var logsToUpdate = await _currentAttendanceLogRepository.GetLogsWithoutEntryTimeAsync(employeeId, month);
        if (!logsToUpdate.Any())
        {
            return;
        }

        var attendanceEntryTimes = await _attendanceLogRepository.GetMonthlyFirstEntryTimesAsync(employeeId, month);

        var scheduleHistory = await _trackRepository.GetScheduleByDateAndByEmployeeIdAsync(employeeId, month);
       
        var currentSchedule = await _employeeRepository.GetScheduleByEmployeeIdAsync(employeeId);

        
        var allSchedules = (scheduleHistory ?? new List<EmployeeScheduleHistory>())
            .Select(s => new {
                s.StartTime,
                FullDateTime = s.ValidFrom 
            })
            .Append(new
            {
                currentSchedule.StartTime,
                FullDateTime = currentSchedule.ModifiedAt 
            })
            .OrderByDescending(s => s.FullDateTime)
            .ToList();

        var updatedLogs = new List<CurrentAttendanceLog>();

        foreach (var log in logsToUpdate)
        {
            if (attendanceEntryTimes.TryGetValue(log.EntryDay, out TimeOnly newEntryTime))
            {
                
                DateTime actualEntryMoment = log.EntryDay.ToDateTime(newEntryTime);
                var applicableSchedule = allSchedules
                 .FirstOrDefault(s => actualEntryMoment >= s.FullDateTime)
                 ?? allSchedules.LastOrDefault();

                if (applicableSchedule != null)
                {
                    log.ScheduledStartTime = applicableSchedule.StartTime;
                }

                log.FirstEntryTime = newEntryTime;
                log.ModifiedAt = DateTime.Now;

                if (log.IsWorkingDay)
                {
                    var calendarDto = new CurrentAttendanceCalendar
                    {
                        EmployeeId = employeeId,
                        EntryDay = log.EntryDay,
                        IsWorkingDay = log.IsWorkingDay,
                        IsJustified = log.IsJustified,
                        FirstEntryTime = newEntryTime,
                        ScheduledStartTime = log.ScheduledStartTime
                    };
                    log.LateArrivalMinutes = CalculateLateMinutes(calendarDto, log.EntryDay.ToDateTime(TimeOnly.MinValue));
                }

                updatedLogs.Add(log);
            }
        }

      
        if (updatedLogs.Any())
        {
            await _currentAttendanceLogRepository.UpdateRangeAsync(updatedLogs);
        }
    }

    private CurrentAttendanceLog MapCalendarDtoToEntity(CurrentAttendanceCalendar dto)
    {

        return new CurrentAttendanceLog
        {
            EmployeeId = dto.EmployeeId,
            EntryDay = dto.EntryDay,
            FirstEntryTime = dto.FirstEntryTime,
            ScheduledStartTime = dto.ScheduledStartTime,
            LastLeavingTime = dto.LastLeavingTime,
            LateArrivalMinutes = dto.LateMinutesTotal,
            RemainingLateMinutes = dto.RemainingLateMinutes,
            WorkedHours = dto.WorkedHours,
            IsJustified = dto.IsJustified,
            IsWorkingDay = dto.IsWorkingDay,
            Description = dto.Description,
        };
    }

    private CurrentAttendanceCalendar MapToCalendarDto(AttendanceLog log, EmployeeSchedule schedule)
    {
        var dto = new CurrentAttendanceCalendar
        {

            EmployeeId = log.EmployeeId,
            EmployeeFullName = log.RawUsername,
            ScheduledStartTime = schedule.StartTime,
            FirstEntryTime = TimeOnly.FromDateTime(log.RecordedTime),
            LastLeavingTime = default,
            WorkedHours = default,
            EntryDay = DateOnly.FromDateTime(log.RecordedTime.Date),
            LateMinutesTotal = default,
            RemainingLateMinutes = default,
            IsJustified = false,
            IsWorkingDay = IsWorkingDay(log.RecordedTime),
            Description = null,
            CalculatedAt = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        return dto;
    }

    private CurrentAttendanceCalendar CreateDefaultCalendarDto(long employeeId, DateTime targetDate, EmployeeSchedule schedule)
    {
        return new CurrentAttendanceCalendar
        {
            EmployeeId = employeeId,
            EmployeeFullName = " ",
            ScheduledStartTime = schedule.StartTime,
            EntryDay = DateOnly.FromDateTime(targetDate),
            FirstEntryTime = default,
            LastLeavingTime = default,
            WorkedHours = 0,
            LateMinutesTotal = 0,
            RemainingLateMinutes = 0,
            IsJustified = false,
            IsWorkingDay = IsWorkingDay(targetDate),
            CalculatedAt = DateTime.Now,
            CreatedAt = DateTime.Now
        };
    }

    private CurrentAttendanceCalendar MapEntityToCalendarDto(CurrentAttendanceLog entity)
    {

        return new CurrentAttendanceCalendar
        {
            EmployeeId = entity.EmployeeId,
            ScheduledStartTime = entity.ScheduledStartTime,
            LateMinutesTotal = entity.LateArrivalMinutes,
            RemainingLateMinutes = entity.RemainingLateMinutes,
            Description = entity.Description,
            IsJustified = entity.IsJustified,
            IsWorkingDay = entity.IsWorkingDay,
            CalculatedAt = entity.CalculatedAt,
            FirstEntryTime = entity.FirstEntryTime,
            LastLeavingTime = entity.LastLeavingTime,
            WorkedHours = entity.WorkedHours,
            EntryDay = entity.EntryDay,
            CreatedAt = entity.CreatedAt,
            ModifiedAt = entity.ModifiedAt
        };
    }

    public async Task<ICollection<CurrentAttendanceCalendar>> GetMonthlyAttendanceCalendarsAsync(long employeeId, DateTime month)
    {
        var allCalendarLogs = await _currentAttendanceLogRepository.GetLogsByEmployeeIdAsync(employeeId);
        var schedule = await _employeeRepository.GetScheduleByEmployeeIdAsync(employeeId);

        var logs = allCalendarLogs.Select(entity => MapEntityToCalendarDto(entity)).ToList();

        var startDate = new DateTime(month.Year, 12, 1).Date;
        var endDate = startDate.AddMonths(1).Date;
        var monthlyLogs = logs
            .Where(log =>
                log.EntryDay.ToDateTime(TimeOnly.MinValue).Date >= startDate &&
                log.EntryDay.ToDateTime(TimeOnly.MinValue).Date < endDate)
            .ToList();

        return monthlyLogs;
    }

    public async Task<int> UpdateEntryTimeManuallyAsync(UpdateEntryTimeDto dto)
    {
        var log = await _currentAttendanceLogRepository.GetLogByEmployeeIdAndEntryDayAsync(dto.EmployeeId, dto.EntryDay);

        if (log == null)
        {

            throw new InvalidOperationException($"Log not found for Employee {dto.EmployeeId} on {dto.EntryDay.ToShortDateString()}");
        }

        var scheduledStartTime = await _employeeRepository.GetScheduleByEmployeeIdAsync(dto.EmployeeId);


        log.FirstEntryTime = TimeOnly.FromTimeSpan(dto.ManualEntryTime);
        log.Description = dto.Description;


        var tempCalendarDto = new CurrentAttendanceCalendar
        {
            FirstEntryTime = log.FirstEntryTime,
            ScheduledStartTime = log.ScheduledStartTime,
            IsWorkingDay = log.IsWorkingDay,

        };

        var minute = log.LateArrivalMinutes = CalculateLateMinutes(tempCalendarDto, log.EntryDay.ToDateTime(TimeOnly.MinValue));

        await _currentAttendanceLogRepository.SaveChangesAsync();
        return minute;
    }

    public async Task<int> UpdateJustificationStatusAsync(UpdateJustificationDto dto)
    {
        var log = await _currentAttendanceLogRepository.GetLogByEmployeeIdAndEntryDayAsync(dto.EmployeeId, dto.EntryDay);

        if (log == null)
        {
            throw new Exception("log not found");
        }

        // Avval logni yangilash
        log.Description = dto.Description;
        log.IsJustified = dto.IsJustified;
        log.ModifiedAt = DateTime.Now;

        // YANGI IsJustified qiymati bilan CurrentAttendanceCalendar ob'ektini yaratish
        var tempCalendarDto = new CurrentAttendanceCalendar
        {
            // CalculateLateMinutes ishiga ta'sir qiladigan barcha kerakli ma'lumotlar kiritilishi kerak
            FirstEntryTime = log.FirstEntryTime,
            ScheduledStartTime = log.ScheduledStartTime,
            Description = log.Description,
            IsJustified = log.IsJustified,
            IsWorkingDay = log.IsWorkingDay,
        };

        // Kechikishni yangi (to'g'ri) holat bo'yicha hisoblash
        var minute = log.LateArrivalMinutes = CalculateLateMinutes(tempCalendarDto, log.EntryDay.ToDateTime(TimeOnly.MinValue));

        await _currentAttendanceLogRepository.UpdateLogAsync(log);
        return minute;
    }
    public async Task<int> UpdateWorkingDayStatusAsync(WorkingDayStatusUpdateDto dto)
    {
        var log = await _currentAttendanceLogRepository.GetLogByEmployeeIdAndEntryDayAsync(dto.EmployeeId, dto.EntryDay);
      
        if (log == null)
        {
            throw new Exception("log not found");
        }

        // Avval logni yangilash
        log.IsWorkingDay = dto.IsWorkingDay;
        log.ModifiedAt = DateTime.Now;
       
        // YANGI IsJustified qiymati bilan CurrentAttendanceCalendar ob'ektini yaratish
        var tempCalendarDto = new CurrentAttendanceCalendar
        {
            IsWorkingDay = log.IsWorkingDay,
            FirstEntryTime = log.FirstEntryTime,
            ScheduledStartTime = log.ScheduledStartTime,
            Description = log.Description,
            IsJustified = log.IsJustified,
        };

        var minute = log.LateArrivalMinutes = CalculateLateMinutes(tempCalendarDto, log.EntryDay.ToDateTime(TimeOnly.MinValue));

        await _currentAttendanceLogRepository.UpdateLogAsync(log);
        return minute;
    }


    public async Task<string?> UpdateDescriptionAsync(DescriptionUpdateDto dto)
    {
        var log = await _currentAttendanceLogRepository.GetLogByEmployeeIdAndEntryDayAsync(dto.EmployeeId, dto.EntryDay);

        if (log == null)
        {
            throw new Exception("log not found");
        }

        log.Description = dto.Description;
        log.ModifiedAt = DateTime.Now;


        await _currentAttendanceLogRepository.UpdateLogAsync(log);

        return log.Description;

    }


}

