using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs;
using AttendanceManagementSystem.Domain.Entities;
using ClosedXML.Excel;
namespace AttendanceManagementSystem.Application.Services;

public class CurrentAttendanceLogCalculationService : ICurrentAttendanceLogCalculationService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAttendanceLogRepository _attendanceLogRepository;
    private readonly ICurrentAttendanceLogRepository _currentAttendanceLogRepository;

    public CurrentAttendanceLogCalculationService(IAttendanceLogRepository attendanceLogRepository, IEmployeeRepository employeeRepository, ICurrentAttendanceLogRepository currentAttendanceLogRepository)
    {
        _attendanceLogRepository = attendanceLogRepository;
        _employeeRepository = employeeRepository;
        _currentAttendanceLogRepository = currentAttendanceLogRepository;
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
        if (date.DayOfWeek == DayOfWeek.Saturday ||
            date.DayOfWeek == DayOfWeek.Sunday)
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

        var scheduledStartDateTime = targetDate.Date + calendarDto.ScheduledStartTime;
        var actualEntryDateTime = targetDate.Date + calendarDto.FirstEntryTime.ToTimeSpan();

        if (actualEntryDateTime <= scheduledStartDateTime)
        {
            return 0;
        }

        var lateDuration = actualEntryDateTime - scheduledStartDateTime;
        return (int)lateDuration.TotalMinutes;
    }

    public async Task<ICollection<CurrentAttendanceCalendar>> GetAndSaveMonthlyAttendanceCalendarAsync(long employeeId, DateTime month)
    {
        var startDate = new DateTime(month.Year, 11, 1);
        int daysInMonth = startDate.AddMonths(1).AddDays(-1).Day;

        // 1. AttendanceLog (Asosiy loglar) ni yuklash
        var allExistingLogs = await _attendanceLogRepository.GetLogsByEmployeeAndMonthAsync(employeeId, month);

        var monthlyCalendar = new List<CurrentAttendanceCalendar>();

        // Asosiy loglarni kunlik tartiblash va faqat birinchi kirishni olish mantiqi
        var dailyLogsGrouped = allExistingLogs
            .GroupBy(log => log.RecordedTime.Date)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    FirstEntry = g.OrderBy(log => log.RecordedTime).First()
                }
            );
                
        var existingLogDates = new HashSet<DateOnly>(
            allExistingLogs
                .Select(log => DateOnly.FromDateTime(log.RecordedTime.Date))
        );
        var schedule = await _employeeRepository.GetScheduleByEmployeeIdAsync(employeeId);

        for (int i = 0; i < daysInMonth; i++)
        {
            var targetDate = startDate.AddDays(i).Date;
            var targetDateOnly = DateOnly.FromDateTime(targetDate);

            if (dailyLogsGrouped.TryGetValue(targetDate, out var dayLog))
            {
                var calendarDto = MapToCalendarDto(dayLog.FirstEntry, schedule);

                calendarDto.LateMinutesTotal = CalculateLateMinutes(calendarDto, targetDate);
                monthlyCalendar.Add(calendarDto);

            }

            else
            {
                var defaultLogDto = CreateDefaultCalendarDto(employeeId, targetDate, schedule);
                defaultLogDto.LateMinutesTotal = CalculateLateMinutes(defaultLogDto, targetDate);
                monthlyCalendar.Add(defaultLogDto);
            }
        }

        var logsToInsert = monthlyCalendar.Select(dto => MapCalendarDtoToEntity(dto)).ToList();

        //await _currentAttendanceLogRepository.DeleteMonthlyLogsAsync(employeeId, month);

        await _currentAttendanceLogRepository.CreateLogAsync(logsToInsert);

        return monthlyCalendar;
    }

   
    public async Task UpdateMonthlyEntryTimesAsync(long employeeId, DateOnly month)
    {
        // Faqat FirstEntryTime kiritilmagan loglarni yuklash (optimallashtirish uchun)
        var logsToUpdate = await _currentAttendanceLogRepository.GetLogsWithoutEntryTimeAsync(employeeId, month);

        if (!logsToUpdate.Any()) return;

        // Shu oy uchun Attendance Loglardan barcha birinchi kirish vaqtlarini olish
        // Qaytish turi: Dictionary<DateOnly, TimeOnly?>
        var attendanceEntryTimes = await _attendanceLogRepository.GetMonthlyFirstEntryTimesAsync(employeeId, month);

        var schedule = await _employeeRepository.GetScheduleByEmployeeIdAsync(employeeId);
        if (schedule == null) return;

        var updatedLogs = new List<CurrentAttendanceLog>();

        foreach (var log in logsToUpdate)
        {
            // 1. Shu kunga kirish vaqti kelganmi?
            // Eslatma: TimeOnly? qilib o'zgartirdim, chunki GetMonthlyFirstEntryTimesAsync shu turni qaytaradi.
            if (attendanceEntryTimes.TryGetValue(log.EntryDay, out TimeOnly newEntryTime))
            {
                // 2. Yangilash
                log.FirstEntryTime = newEntryTime;
                log.ModifiedAt = DateTime.Now;

                // Kechikishni hisoblash
                if (log.IsWorkingDay)
                {
                    // ** O'zgartirish 1: CalculateLateMinutes uchun DTO yaratish **
                    // Vaqtincha DTO yaratiladi, chunki CalculateLateMinutes uni talab qiladi.
                    var calendarDtoForCalculation = new CurrentAttendanceCalendar
                    {
                        // CalculateLateMinutes talab qiladigan maydonlarni to'ldiramiz:
                        EmployeeId = employeeId,
                        EntryDay = log.EntryDay, // Sanani DateTime formatida (agar DTO talab qilsa)
                        IsWorkingDay = log.IsWorkingDay,
                        IsJustified=log.IsJustified,
                        FirstEntryTime = newEntryTime,
                        ScheduledStartTime = schedule.StartTime
                    };

                    // ** O'zgartirish 2: CalculateLateMinutes chaqiruvi **
                    // Endi to'g'ri DTO va TargetDate'ni uzatamiz.
                    log.LateArrivalMinutes = CalculateLateMinutes(calendarDtoForCalculation, log.EntryDay.ToDateTime(TimeOnly.MinValue));
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
            LastLeavingTime = dto.LastLeavingTime,
            LateArrivalMinutes = dto.LateMinutesTotal,
            RemainingLateMinutes = dto.RemainingLateMinutes,
            WorkedHours = dto.WorkedHours,
            IsJustified = dto.IsJustified,
            IsWorkingDay = dto.IsWorkingDay,
            Description = dto.Description,
            CalculatedAt = DateTime.Now,
            CreatedAt = DateTime.Now,
            ModifiedAt = null

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

    private CurrentAttendanceCalendar MapEntityToCalendarDto(CurrentAttendanceLog entity, EmployeeSchedule schedule)
    {

        return new CurrentAttendanceCalendar
        {
            EmployeeId = entity.EmployeeId,
            EmployeeFullName = "UserName",
            ScheduledStartTime = schedule.StartTime,
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

        var logs = allCalendarLogs.Select(entity => MapEntityToCalendarDto(entity, schedule)).ToList();

        var startDate = new DateTime(month.Year, 11, 1).Date;
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

        var scheduledStartTime = await _employeeRepository.GetScheduleByEmployeeIdAsync(dto.EmployeeId)
            .ContinueWith(t => t.Result?.StartTime);

        if (!scheduledStartTime.HasValue)
        {
            scheduledStartTime = TimeSpan.Zero;
        }

        log.FirstEntryTime = TimeOnly.FromTimeSpan(dto.ManualEntryTime);
        log.Description = dto.Description;
        log.ModifiedAt= DateTime.Now;
        
        var tempCalendarDto = new CurrentAttendanceCalendar
        {
            FirstEntryTime = log.FirstEntryTime,
            ScheduledStartTime = scheduledStartTime.Value,
            IsWorkingDay=log.IsWorkingDay,
        };

        var minute=log.LateArrivalMinutes = CalculateLateMinutes(tempCalendarDto, log.EntryDay.ToDateTime(TimeOnly.MinValue));

        await _currentAttendanceLogRepository.SaveChangesAsync();
        return minute;
    }


    public async Task<int> UpdateJustificationStatusAsync(UpdateJustificationDto dto)
    {
        var log = await _currentAttendanceLogRepository.GetLogByEmployeeIdAndEntryDayAsync(dto.EmployeeId, dto.EntryDay);
        var scheduledStartTime = await _employeeRepository.GetScheduleByEmployeeIdAsync(dto.EmployeeId)
          .ContinueWith(t => t.Result?.StartTime);
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
            ScheduledStartTime = scheduledStartTime.Value,
            Description = log.Description,
            IsJustified = log.IsJustified,
            IsWorkingDay= log.IsWorkingDay,
        };

        // Kechikishni yangi (to'g'ri) holat bo'yicha hisoblash
        var minute = log.LateArrivalMinutes = CalculateLateMinutes(tempCalendarDto, log.EntryDay.ToDateTime(TimeOnly.MinValue));

        await _currentAttendanceLogRepository.UpdateLogAsync(log);
        return minute;
    }
    public async Task<int> UpdateWorkingDayStatusAsync(WorkingDayStatusUpdateDto dto)
    {
        var log = await _currentAttendanceLogRepository.GetLogByEmployeeIdAndEntryDayAsync(dto.EmployeeId, dto.EntryDay);
        var scheduledStartTime = await _employeeRepository.GetScheduleByEmployeeIdAsync(dto.EmployeeId)
          .ContinueWith(t => t.Result?.StartTime);
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
            ScheduledStartTime = scheduledStartTime.Value,
            Description = log.Description,
            IsJustified = log.IsJustified,
        };

        // Kechikishni yangi (to'g'ri) holat bo'yicha hisoblash
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

        // Avval logni yangilash
        log.Description = dto.Description;
        log.ModifiedAt = DateTime.Now;


        await _currentAttendanceLogRepository.UpdateLogAsync(log);
       
        return log.Description;
        
    }


}

