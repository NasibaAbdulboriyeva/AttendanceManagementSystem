using AttendanceManagementSystem.Api.Models;
using AttendanceManagementSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Api.Controllers
{
   
    [Authorize]
    public class UserAttendanceController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly ICurrentAttendanceLogCalculationService _calculationService;
        private readonly IUserService _userService; 

        public UserAttendanceController(
            IEmployeeService employeeService,
            ICurrentAttendanceLogCalculationService calculationService,
            IUserService userService) 
        {
            _employeeService = employeeService;
            _calculationService = calculationService;
            _userService = userService;
        }


        [HttpGet]
        public async Task<IActionResult> MyCalendar(int year, int month)
        {
            // 1. Joriy foydalanuvchi ID'sini olish
            // Eslatma: Haqiqiy ilovada bu Service orqali yoki User.Identity/Claims orqali olinadi.
            var currentEmployeeId = await _userService.GetEmployeeIdFromCurrentUserAsync();

            if (currentEmployeeId <= 0)
            {
                return Unauthorized(); // ID topilmasa
            }

            var targetMonth = new DateTime(
                year == 0 ? DateTime.Now.Year : year,
                month == 0 ? DateTime.Now.Month : month,
                1);

            var employee = await _employeeService.GetEmployeeByIdAsync(currentEmployeeId);

            // 2. Faqat o'ziga tegishli davomat ma'lumotlarini olish
            var logs = await _calculationService.GetMonthlyAttendanceCalendarsAsync(currentEmployeeId, targetMonth);

            var viewModel = new AttendanceCalendarViewModel
            {
                TargetMonth = targetMonth,
                EmployeeFullName = employee?.UserName ?? "Mening davomatim",
                MonthlyLogs = logs,
                // Foydalanuvchi o'zi uchun kiritishga ruxsat bermaslik uchun
            };

            return View("Calendar", viewModel); // Adminning Calendar view'idan foydalanishi mumkin
        }



        [HttpGet]
        public async Task<IActionResult> MyLateArrivalsSummary(DateTime? targetMonth)
        {
            // 1. Joriy foydalanuvchi ID'sini olish
            var currentEmployeeId = await _userService.GetEmployeeIdFromCurrentUserAsync();

            if (currentEmployeeId <= 0)
            {
                return Unauthorized();
            }

            var month = targetMonth.HasValue
                ? new DateTime(targetMonth.Value.Year, targetMonth.Value.Month, 1)
                : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // Faqat joriy xodim ma'lumotlarini olish
            var currentEmployeeRaw = await _employeeService.GetEmployeeByIdAsync(currentEmployeeId);

            if (currentEmployeeRaw == null)
            {
                return NotFound("Xodim ma'lumotlari topilmadi.");
            }

            var employeesSummary = new List<EmployeeSummary>();

            var employeeSummary = new EmployeeSummary
            {
                EmployeeId = currentEmployeeRaw.EmployeeId,
                FullName = currentEmployeeRaw.UserName,
                TotalLateMinutes = 0
            };

            // 2. Faqat joriy xodim uchun kechikish xulosasini olish
            var lateSummaryDictionary = await _calculationService.GetEmployeesLateSummaryAsync(month);

            if (lateSummaryDictionary.TryGetValue(employeeSummary.EmployeeId, out int totalMinutes))
            {
                employeeSummary.TotalLateMinutes = totalMinutes;
            }

            employeesSummary.Add(employeeSummary);


            var model = new AttendanceSummaryViewModel
            {
                Employees = employeesSummary, // Faqat 1 ta xodim bo'ladi
                TargetMonth = month,
            };

            return View("LateArrivalsSummary", model); // Adminning LateArrivalsSummary view'idan foydalanishi mumkin
        }

        // 💡 Qolgan metodlar (masalan, ScheduleSetup, Sync) faqat Admin Controller'da qolishi kerak.
    }

    // 💡 Eslatma: IUserService interfeysi va uning implementatsiyasini yaratishingiz kerak.
    // Masalan:
    public interface IUserService
    {
        Task<int> GetEmployeeIdFromCurrentUserAsync();
    }
}