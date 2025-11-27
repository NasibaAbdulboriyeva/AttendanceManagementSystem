using AttendanceManagementSystem.Api.Models;
using AttendanceManagementSystem.Application.DTOs;
using AttendanceManagementSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Api.Controllers
{
    public class AdminAttendanceController : Controller
    {
        private readonly IAttendanceLogService _logService;
        private readonly IEmployeeService _employeeService;
        private readonly ICurrentAttendanceLogCalculationService _calculationService;
        public AdminAttendanceController(IAttendanceLogService logService, IEmployeeService employeeService, ICurrentAttendanceLogCalculationService calculationService)
        {
            _logService = logService;
            _employeeService = employeeService;
            _calculationService = calculationService;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View(new SyncViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SyncViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                int savedCount = await _logService.SyncAttendanceLogsAsync();
                model.SyncedCount = savedCount;
                model.Message = $"✅ Логи успешно синхронизированы! {savedCount} новых логов сохранено в базу данных.";
            }
            catch (Exception ex)
            {
                model.Message = $"❌ Ошибка при синхронизации логов:{ex.Message}";
                // Logging mexanizmini qo'shing (masalan, ILogger orqali)
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult EmployeeSync()
        {
            // Xodim sinxronizatsiyasi uchun ham xuddi shu model ishlatilishi mumkin, yoki yangi model yaratishingiz mumkin.
            return View(new SyncViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeSync(SyncViewModel model)
        {
            // Lock ID bu sinxronizatsiya uchun majburiy emas (chunki EmployeeService ichida 0 yuboryapsiz)
            // Lekin agar TTLock API'ga har doim LockId yuborish kerak bo'lsa, bu yerda validatsiya qiling.

            // if (!ModelState.IsValid) { return View(model); } 

            try
            {
                // Service metodini chaqiramiz: Cards va Fingerprint'larni sinxronlash
                var (cardsSynced, fingerprintsSynced) = await _employeeService.SyncEmployeeDataAsync();
                // Natijani modelga yozamiz
                model.SyncedCount = cardsSynced + fingerprintsSynced;
                model.Message = $"✅ Сотрудники успешно синхронизированы! " +
                $"Всего {model.SyncedCount} записей (Карты: {cardsSynced}, Отпечатки: {fingerprintsSynced}) обновлено/добавлено."; // RUSCHA
            }
            catch (Exception ex)
            {
                // Xato yuz bersa
                model.Message = $"❌ Ошибка при синхронизации сотрудников: {ex.Message}";
                // Logging mexanizmini qo'shish tavsiya etiladi.
            }
            // Xabar bilan View ni qayta ko'rsatamiz
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeList()
        {
          
            var allEmployees = await _employeeService.GetAllActiveEmployeesAsync();

            var viewModel = new EmployeeListViewModel();


            viewModel.Employees = allEmployees
                .Select(e => new EmployeeListItem
                {
                    FullName = e.UserName,
                }).ToList();

            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> ViewCalendar(long employeeId, int year, int month)
        {
            // Agar ma'lumotlar uzatilmasa, Index'ga qaytaramiz (yoki xato xabari)
            if (employeeId == 0) return RedirectToAction("EmployeeList");

            // O'tilgan yoki default oy/yilni o'rnatish
            var targetMonth = new DateTime(
                year == 0 ? DateTime.Now.Year : year,
                month == 0 ? DateTime.Now.Month : month, 1);

            // Xodim ma'lumotlarini olish
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                return NotFound("Сотрудник не найден.");
            }

            // 1. Hisob-kitob Servisini chaqirish (UPSERT amalga oshiriladi)
            var logs = await _calculationService.GetAndSaveMonthlyAttendanceCalendarAsync(employeeId, targetMonth);

            // 2. View Modelni to'ldirish
            var viewModel = new AttendanceCalendarViewModel
            {
                TargetMonth = targetMonth,
                EmployeeFullName = employee.UserName,
                MonthlyLogs = logs
            };

            return View("Calendar", viewModel); // Calendar.cshtml View'ga yo'naltiramiz
        }
        [HttpGet]
        public async Task<IActionResult> Inactivate(string username)
        {
            var id = await _employeeService.GetEmployeeIdByUsernameAsync(username);

            await _employeeService.DeactivateEmployeeAsync(id);

            return RedirectToAction("EmployeeList");

        }
        [HttpGet]
        public async Task<IActionResult> ScheduleSetup()
        {
            // 1. Barcha xodimlarni olish
            var allEmployeesDto = await _employeeService.GetAllActiveEmployeesAsync();

            var viewModel = new EmployeeScheduleViewModel();

            // 2. Har bir xodim uchun jadvalni yuklash
            foreach (var employeeDto in allEmployeesDto.OrderBy(e => e.UserName))
            {
                // 2.1. Mavjud jadvalni olish
                var scheduleDto = await _employeeService.GetEmployeeScheduleByEmployeeIdAsync(employeeDto.EmployeeId);

                var item = new ScheduleListItem
                {
                    FullName = employeeDto.UserName
                };

                if (scheduleDto != null)
                {
                    // Mavjud jadvalni DTO dan View Modelga o'tkazish (UPDATE uchun)
                    item.StartTime = scheduleDto.StartTime;
                    item.EndTime = scheduleDto.EndTime;
                    item.LimitInMinutes = scheduleDto.LimitInMinutes;
                    item.EmployementType = scheduleDto.EmployementType;
                }

                viewModel.Schedules.Add(item);
            }

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleSetup(EmployeeScheduleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Message = "❌ Ba'zi maydonlar noto'g'ri to'ldirilgan. Iltimos, tekshiring.";
                return View(model);
            }

            int updatedCount = 0;

            try
            {
                foreach (var item in model.Schedules)
                {
                    var emploeeId = await _employeeService.GetEmployeeIdByUsernameAsync(item.FullName);
                    var scheduleDto = new EmployeeScheduleDto
                    {
                        EmployeeId = emploeeId,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        LimitInMinutes = item.LimitInMinutes,
                        EmployementType = item.EmployementType
                    };
                    if (item.EmployeeScheduleId == 0)
                    {
                        await _employeeService.AddEmployeeScheduleAsync(scheduleDto);
                    }
                    else
                    {
                        await _employeeService.UpdateEmployeeScheduleAsync(scheduleDto);

                    }

                    updatedCount++;
                }

                model.Message = $"✅ Barcha {updatedCount} ta jadval ma'lumotlari muvaffaqiyatli saqlandi/yangilandi!";
            }
            catch (Exception ex)
            {
                model.Message = $"❌ Saqlashda xatolik yuz berdi: {ex.Message}";
                // Xato bo'lsa ham, formani qaytaramiz (shu sababli 'return View(model)')
            }

            return View(model);
        }
    }
}