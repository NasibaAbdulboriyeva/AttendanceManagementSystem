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

    public Task<ICollection<CurrentAttendanceCalendar>> GetCalculatedAttendanceForPeriodAsync(string employeeCode, DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<CurrentAttendanceCalendar>> GetLateArrivalsForPeriodAsync(string username, DateTime month)
    {
        throw new NotImplementedException();
    }
    // CurrentAttendanceLogRepository.cs ichida qo'shimcha metod
 
    public async Task<ICollection<CurrentAttendanceCalendar>> GetAndSaveMonthlyAttendanceCalendarAsync(long employeeId, DateTime month)
    {
        var startDate = new DateTime(month.Year, month.Month, 1);
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

        // 2. Oyning har bir kuni uchun kalendar yozuvlarini yaratish (log bor yoki default)
        for (int i = 0; i < daysInMonth; i++)
        {
            var targetDate = startDate.AddDays(i).Date;
            var targetDateOnly = DateOnly.FromDateTime(targetDate);

            if (dailyLogsGrouped.TryGetValue(targetDate, out var dayLog))
            {
                var calendarDto = MapToCalendarDto(dayLog.FirstEntry);
                // BU YERDA KECH QOLISH HISOBLASH MANTIQI QO'SHILADI
                // Masalan: CalculateLateness(calendarDto, targetDate)
                monthlyCalendar.Add(calendarDto);
            }
            else
            {
                var defaultLogDto = CreateDefaultCalendarDto(employeeId, targetDate);
                monthlyCalendar.Add(defaultLogDto);
            }
        }

        // 3. CurrentAttendanceLog Entity'lariga konvertatsiya qilish
        var logsToInsert = monthlyCalendar.Select(dto => MapCalendarDtoToEntity(dto)).ToList();

        // --- ✅ MUHIM TUZATISH: UPSERT mantiqi (DELETE + INSERT) ---

        // 1. Avval shu oyga oid barcha mavjud yozuvlarni O'CHIRIB tashlash
        await _currentAttendanceLogRepository.DeleteMonthlyLogsAsync(employeeId, month);

        // 2. So'ngra yangi hisoblangan barcha yozuvlarni QAYTA SAQLASH (INSERT)
        await _currentAttendanceLogRepository.CreateLogAsync(logsToInsert);

        // --- Tuzatish tugadi ---

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

    private CurrentAttendanceCalendar MapToCalendarDto(AttendanceLog log)
    {
        var dto = new CurrentAttendanceCalendar
        {
            EmployeeId = log.EmployeeId,
            EmployeeFullName = log.RawUsername,
            ScheduledStartTime = TimeSpan.FromHours(9),
            FirstEntryTime = TimeOnly.FromDateTime(log.RecordedTime),
            LastLeavingTime = default,
            WorkedHours = 0,
            EntryDay = DateOnly.FromDateTime(log.RecordedTime.Date),
            LateMinutesTotal = 0,
            RemainingLateMinutes = 0,
            IsJustified = false,
            Description = null,
            CalculatedAt = DateTime.Now,
            CreatedAt = DateTime.Now
        };

        return dto;
    }

    private CurrentAttendanceCalendar CreateDefaultCalendarDto(long employeeId, DateTime targetDate)
    {
        return new CurrentAttendanceCalendar
        {
            EmployeeId = employeeId,
            EmployeeFullName = " ",
            ScheduledStartTime =default,
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
}

