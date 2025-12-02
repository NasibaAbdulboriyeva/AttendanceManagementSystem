using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs;
using AttendanceManagementSystem.Domain.Entities;
using DocumentFormat.OpenXml.InkML;

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

    public Task<ICollection<CurrentAttendanceCalendar>> GetLateArrivalsForPeriodAsync(string username, DateTime month)
    {
        throw new NotImplementedException();
    }

    public async Task ProcessAllEmployeesMonthlyAttendanceAsync(DateTime month)
    {
        var activeEmployeeIds = await _employeeRepository.GetAllActiveEmployeesAsync();

        foreach (var employee in activeEmployeeIds)
        {

            await GetAndSaveMonthlyAttendanceCalendarAsync(employee.EmployeeId, month);
        }
    }
    public  int CalculateLateMinutes (CurrentAttendanceCalendar calendarDto, DateTime targetDate)
    {
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

        // ... (existingLogDates ishlatilmasa ham mayli, olib tashlanmadi)
        var existingLogDates = new HashSet<DateOnly>(
            allExistingLogs
                .Select(log => DateOnly.FromDateTime(log.RecordedTime.Date))
        );
        var schedule = await _employeeRepository.GetScheduleByEmployeeIdAsync(employeeId);

        // 2. Oyning har bir kuni uchun kalendar yozuvlarini yaratish (log bor yoki default)
        for (int i = 0; i < daysInMonth; i++)
        {
            var targetDate = startDate.AddDays(i).Date;
            var targetDateOnly = DateOnly.FromDateTime(targetDate);

            if (dailyLogsGrouped.TryGetValue(targetDate, out var dayLog))
            {
                var calendarDto = MapToCalendarDto(dayLog.FirstEntry,schedule);
                // BU YERDA KECH QOLISH HISOBLASH MANTIQI QO'SHILADI
                // Masalan: CalculateLateness(calendarDto, targetDate)
                calendarDto.LateMinutesTotal=CalculateLateMinutes(calendarDto, targetDate);
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

        await _currentAttendanceLogRepository.DeleteMonthlyLogsAsync(employeeId, month);

        await _currentAttendanceLogRepository.CreateLogAsync(logsToInsert);
        
        return monthlyCalendar;
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
            Description = dto.Description,
            CalculatedAt = DateTime.Now,
            CreatedAt = DateTime.Now,
            ModifiedAt = null

        };
    }

    private CurrentAttendanceCalendar MapToCalendarDto(AttendanceLog log,EmployeeSchedule schedule)
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
            Description = null,
            CalculatedAt = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        return dto;
    }

    private   CurrentAttendanceCalendar CreateDefaultCalendarDto(long employeeId, DateTime targetDate,EmployeeSchedule schedule)
    {
        return new CurrentAttendanceCalendar
        {
            EmployeeId = employeeId,
            EmployeeFullName = " ",
            ScheduledStartTime =schedule.StartTime,
            EntryDay = DateOnly.FromDateTime(targetDate),
            FirstEntryTime = default,
            LastLeavingTime = default,
            WorkedHours = 0,
            LateMinutesTotal = 0,
            RemainingLateMinutes = 0,
            IsJustified = false,
            CalculatedAt = DateTime.Now,
            CreatedAt = DateTime.Now
        };
    }
    private CurrentAttendanceCalendar MapEntityToCalendarDto(CurrentAttendanceLog entity, EmployeeSchedule schedule)
    {
        // Izoh: ScheduledStartTime va EmployeeFullName ma'lumotlari
        // Entityda mavjud emas. Ular DTO'da to'ldirish uchun boshqa manbalardan
        // olinishi yoki bu yerda default qiymatlar qo'yilishi kerak.

        return new CurrentAttendanceCalendar
        {
            EmployeeId = entity.EmployeeId,

            // Xodimning to'liq ismi (EmployeeFullName) va jadval boshlanish vaqti (ScheduledStartTime)
            // boshqa repository'dan olinishi kerak. Hozircha default/bo'sh qoldiriladi.
            EmployeeFullName = "UserName",
            ScheduledStartTime = schedule.StartTime,

            // LateArrivalMinutes => LateMinutesTotal ga mos keladi
            LateMinutesTotal = entity.LateArrivalMinutes,
            RemainingLateMinutes = entity.RemainingLateMinutes,
            Description = entity.Description,
            IsJustified = entity.IsJustified,
            CalculatedAt = entity.CalculatedAt,
            FirstEntryTime = entity.FirstEntryTime,
            LastLeavingTime = entity.LastLeavingTime,
            WorkedHours = entity.WorkedHours,
            EntryDay = entity.EntryDay,
            CreatedAt = entity.CreatedAt,
            ModifiedAt = entity.ModifiedAt
        };
    }
    // Service Qatlami (Samarasiz yondashuv - tavsiya etilmaydi)
    public async Task<ICollection<CurrentAttendanceCalendar>> GetMonthlyAttendanceCalendarsAsync(long employeeId, DateTime month)
    {
        // 1. Repozitoriyani chaqirish - OY PARAMETRSİZ (Bu Repozitoriya BARCHA loglarni qaytaradi)
        var allCalendarLogs = await _currentAttendanceLogRepository.GetLogsByEmployeeIdAsync(employeeId);
        var schedule = await _employeeRepository.GetScheduleByEmployeeIdAsync(employeeId);

        var logs = allCalendarLogs.Select(entity => MapEntityToCalendarDto(entity,schedule)).ToList();
       
        var startDate = new DateTime(month.Year, 11, 1).Date;
        var endDate = startDate.AddMonths(1).Date;
        // 2. Service ichida, ya'ni Xotirada (In-Memory) filtrlash!
        var monthlyLogs = logs
            .Where(log =>
                log.EntryDay.ToDateTime(TimeOnly.MinValue).Date >= startDate &&
                log.EntryDay.ToDateTime(TimeOnly.MinValue).Date < endDate)
            .ToList();

        return monthlyLogs;
    }

    public async Task UpdateEntryTimeManuallyAsync(UpdateEntryTimeDto dto)
    {
        // 1. Logni topish
        var log = await _currentAttendanceLogRepository.GetLogByEmployeeIdAndEntryDayAsync(dto.EmployeeId,dto.EntryDay);
           
        if (log == null)
        {
            // Eslatma: Agar log topilmasa, admin kiritgan ma'lumotlar bilan yangi log yaratish mantiqi qo'shilishi KEREK.
            // Hozir soddalashtirilgan, mavjud logni yangilash mantiqini yozamiz.
            throw new InvalidOperationException($"Log not found for Employee {dto.EmployeeId} on {dto.EntryDay.ToShortDateString()}");
        }

        // 2. Xodimning belgilangan jadvalini yuklash
        // Bu vaqt kechikishni hisoblash uchun kerak.
        var scheduledStartTime = await _employeeRepository.GetScheduleByEmployeeIdAsync(dto.EmployeeId)
            .ContinueWith(t => t.Result?.StartTime);

        if (!scheduledStartTime.HasValue)
        {
            // Jadval topilmasa 0 bo'lsin deb faraz qilamiz
            scheduledStartTime = TimeSpan.Zero;
        }

        
            log.FirstEntryTime = TimeOnly.FromTimeSpan(dto.ManualEntryTime);
            log.Description = dto.Description;


            // DTO'ni vaqtinchalik DTOga o'tkazish, chunki CalculateLateMinutes DTO kutadi
            var tempCalendarDto = new CurrentAttendanceCalendar
            {
                FirstEntryTime = log.FirstEntryTime, // Agar FirstEntryTime TimeOnly bo'lmasa, uni to'g'irlash kerak
                ScheduledStartTime = scheduledStartTime.Value
            };

            // 4. Kechikishni qayta hisoblash
            // CalculateLateMinutes metodida Date/EntryDay kerak, log.EntryDay ni uzatamiz
            log.LateArrivalMinutes = CalculateLateMinutes(tempCalendarDto, log.EntryDay.ToDateTime(TimeOnly.MinValue));
        
        // 5. Bazaga saqlash
        await _currentAttendanceLogRepository.SaveChangesAsync();
    }

    // ====================================================================
    // 2. ✅ Sababini Belgilash (UpdateJustificationStatusAsync)
    // ====================================================================

    public async Task UpdateJustificationStatusAsync(UpdateJustificationDto dto)
    {
        // 1. Logni topish
        var log = await _currentAttendanceLogRepository.GetLogByEmployeeIdAndEntryDayAsync(dto.EmployeeId,dto.EntryDay);

        if (log == null)
        {
            // Agar log topilmasa, bu kun uchun log yo'q (masalan, dam olish kuni)
            // Lekin siz faqat jadvalda ko'rsatilgan mavjud loglarni o'zgartirmoqchisiz.
            return;
        }
        
        // 2. Log'ni yangilash
        log.IsJustified = dto.IsJustified;

        // Eslatma: Kechikishni qayta hisoblash kerak emas, chunki IsJustified faqat limit hisobiga ta'sir qiladi.

        // 3. Bazaga saqlash
        await _currentAttendanceLogRepository.SaveChangesAsync();
    }
}

