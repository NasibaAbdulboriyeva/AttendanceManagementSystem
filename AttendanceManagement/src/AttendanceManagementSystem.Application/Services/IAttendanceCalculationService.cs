using AttendanceManagementSystem.Domain.Entities;


namespace AttendanceManagementSystem.Application.Services
{
    public interface IAttendanceCalculationService
    {
        // Barcha xodimlar uchun ma'lum bir sana bo'yicha hisobotni hisoblash va saqlash
        Task<int> ProcessDailyAttendanceAsync(DateTime date);

        // Xodim uchun ma'lum bir oyga doir barcha ma'lumotlarni hisoblash
        Task<EmployeeSummaryDto> GetMonthlySummaryAsync(long employeeId, int year, int month);

        // Kechikish/Erta ketish kabi individual log yozuvini tekshirish
        Task<CalculatedLogDto> CalculateLogStatusAsync(AttendanceLog log);
    }
}
