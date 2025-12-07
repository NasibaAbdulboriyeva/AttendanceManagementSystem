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
                
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult EmployeeSync()
        {
           
            return View(new SyncViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeSync(SyncViewModel model)
        {

            try
            {

                var (cardsSynced, fingerprintsSynced) = await _employeeService.SyncEmployeeDataAsync();
                // Natijani modelga yozamiz
                model.SyncedCount = cardsSynced + fingerprintsSynced;
                model.Message = $"✅ Сотрудники успешно синхронизированы! " +
                $"Всего {model.SyncedCount} записей (Карты: {cardsSynced}, Отпечатки: {fingerprintsSynced}) обновлено/добавлено."; // RUSCHA
            }
            catch (Exception ex)
            {
             
                model.Message = $"❌ Ошибка при синхронизации сотрудников: {ex.Message}";
             
            }
           
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
        public async Task<IActionResult> ViewCreateCalendarForAll(int year, int month)
        {
            var targetMonth = new DateTime(
                year == 0 ? DateTime.Now.Year : year,
                month == 0 ? 11 : month, 1);

            await _calculationService.ProcessAllEmployeesMonthlyAttendanceAsync(targetMonth);
            var emptyViewModel = new AttendanceCalendarViewModel
            {
                TargetMonth = targetMonth, 
               
            };
            return View("Calendar", emptyViewModel);
        }


        [HttpGet]
        [HttpPost] // Oyni tanlash formasi POST so'rovi yuborishi uchun
       public async Task<IActionResult> LateArrivalsSummary(DateTime? targetMonth)
        {
            // Oyni aniqlash (Default: joriy oy)
            var month = targetMonth.HasValue
                ? new DateTime(targetMonth.Value.Year, targetMonth.Value.Month, 1)
                : new DateTime(DateTime.Now.Year, 11, 1);

            // 1. Barcha xodimlar ro'yxatini yuklash va EmployeeSummary formatiga o'tkazish (Xatolik tuzatildi)
            var allEmployeesRaw = await _employeeService.GetAllActiveEmployeesAsync();

            var employeesSummary = allEmployeesRaw
                .Select(e => new EmployeeSummary
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.UserName,
                    TotalLateMinutes = 0 // Boshlang'ich qiymat
                })
                .ToList();

            // 2. Barcha xodimlar uchun kech qolish minutlarini hisoblash
            // YANGI YONDASHUV: Endi servis metodi butun employeesSummary ro'yxatini qabul qiladi
            // va har bir xodim uchun TotalLateMinutes ni to'g'ridan-to'g'ri yangilaydi.
            // (Agar servis metodi Dictionary qaytarsa, 3-qadam ishlaydi)

            // Servis metodini Dictionary qaytarish mantig'ini saqlab qolgan holda chaqiramiz:
            var lateSummaryDictionary = await _calculationService.GetEmployeesLateSummaryAsync(month);

            // 3. Xodimlar ro'yxatini kech qolish minutlari bilan birlashtirish (Dictionary yordamida)
            foreach (var employee in employeesSummary)
            {
                if (lateSummaryDictionary.TryGetValue(employee.EmployeeId, out int totalMinutes))
                {
                    // Agar kech qolish minutlari topilsa, EmployeeSummary obyektiga yuklash
                    employee.TotalLateMinutes = totalMinutes;

                }

            }

            // 4. ViewModel yaratish va natijani View'ga yuborish
            var model = new AttendanceSummaryViewModel
            {
                // To'g'rilangan ro'yxatni uzatish
                Employees = employeesSummary,
                TargetMonth = month
            };

            return View(model);
        }

        // Eslatma: Eski GetLateMinutesForAllEmployees metodini olib tashlang!


        [HttpGet]
        public async Task<IActionResult> ViewCalendar(string username, int year, int month)
        {
            if (username == null)
            {
                return RedirectToAction("EmployeeList");

            }

            var targetMonth = new DateTime(
                year == 0 ? DateTime.Now.Year : year,
                month == 0 ? 11: month, 1);
            
            var employeeId = await _employeeService.GetEmployeeIdByUsernameAsync(username);
            var employee =await _employeeService.GetEmployeeByIdAsync(employeeId);

 
            var logs=await _calculationService.GetMonthlyAttendanceCalendarsAsync(employeeId, targetMonth);

            var viewModel = new AttendanceCalendarViewModel
            {
                TargetMonth = targetMonth,
                EmployeeFullName = employee.UserName,
                MonthlyLogs = logs
            };

            return View("Calendar", viewModel);
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
            var allEmployeesDto = await _employeeService.GetAllActiveEmployeesAsync();
            var viewModel = new EmployeeScheduleViewModel();

            // 1. SuccessMessage ni tekshirish va uni ViewModelga yuklash
            if (TempData["SuccessMessage"] is string successMessage)
            {
                viewModel.Message = successMessage;
            }
            // 2. ErrorMessage ni tekshirish va uni ViewModelga yuklash
            else if (TempData["ErrorMessage"] is string errorMessage)
            {
                viewModel.Message = errorMessage;
            }

            foreach (var employeeDto in allEmployeesDto.OrderBy(e => e.UserName))
            {
                // ... (xodimlar grafigini yuklash logikasi o'zgarmaydi) ...

                var scheduleDto = await _employeeService.GetEmployeeScheduleByEmployeeIdAsync(employeeDto.EmployeeId);

                var item = new ScheduleListItem
                {
                    FullName = employeeDto.UserName,
                    // Xodim IDsini saqlashni unutmang, bu POST so'rovi uchun zarur!
                    // Agar DTO'da EmployeeId bo'lsa, uni ScheduleListItem'ga ham qo'shish kerak.
                    // Hozircha FullName orqali olishni taxmin qilamiz, ammo bu samarali emas.
                };

                if (scheduleDto != null)
                {
                    item.EmployeeScheduleId = scheduleDto.EmployeeScheduleId; // IDni saqlash juda muhim
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
      
        public async Task<IActionResult> UpdateEntryTime([FromBody] UpdateEntryTimeDto dto)
        {
           
            await _calculationService.UpdateEntryTimeManuallyAsync(dto);

            return Ok(new { success = true });
        }

        [HttpPost]
        
        public async Task<IActionResult> UpdateJustificationStatus([FromBody] UpdateJustificationDto dto)
        {
            
            await _calculationService.UpdateJustificationStatusAsync(dto);

            return Ok(new { success = true });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleSetup(EmployeeScheduleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Message = "❌ Некоторые поля заполнены неправильно. Пожалуйста, проверьте.";
                return View(model);
            }

            int updatedCount = 0;

            try
            {
                foreach (var item in model.Schedules)
                {
                    // Eslatma: GetEmployeeIdByUsernameAsync har bir iteratsiyada chaqirilmasligi uchun 
                    // ma'lumotlar to'g'ri bog'langanligiga ishonch hosil qilish kerak.
                    // Hozirgi kodda bu amal saqlanib qoladi.
                    var emploeeId = await _employeeService.GetEmployeeIdByUsernameAsync(item.FullName);

                    var scheduleDto = new EmployeeScheduleDto
                    {
                        // EmployeeId ni to'g'ridan-to'g'ri Model.Schedules[@i].EmployeeId dan olish yaxshiroq, 
                        // ammo hozircha sizning FullName orqali olish kodingiz saqlanadi.
                        EmployeeId = emploeeId,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        LimitInMinutes = item.LimitInMinutes,
                        EmployementType = item.EmployementType
                    };

                    // EmployeeScheduleId ni ham DTOga o'tkazish kerak, agar yangilanish bo'lsa.
                    // Agar sizning UpdateEmployeeScheduleAsync metodi faqat EmployeeId bo'yicha yangilasa, 
                    // bu yerda EmployeeScheduleId kerak bo'lmasligi mumkin.
                    // Agar ID kerak bo'lsa, uni DTOga qo'shing. (Asl kodda bu yo'q edi, shuning uchun qo'shmadim.)
                    var schedule = await _employeeService.GetEmployeeScheduleByEmployeeIdAsync(emploeeId);
                    if (schedule.EmployeeScheduleId== 0)
                    {
                        await _employeeService.AddEmployeeScheduleAsync(scheduleDto);
                    }
                    else
                    {
                        // Agar sizning DTO'ngiz EmployeeScheduleId ni talab qilsa:
                        // scheduleDto.EmployeeScheduleId = item.EmployeeScheduleId;
                        await _employeeService.UpdateEmployeeScheduleAsync(scheduleDto);
                    }

                    updatedCount++;
                }

                // Muvaffaqiyat xabarini TempData orqali saqlaymiz, chunki biz Redirect qilmoqdamiz.
                TempData["SuccessMessage"] = $"✅ Все {updatedCount} расписаний успешно сохранены или обновлены!";

                // ******************************************************************
                // 🔥 Asosiy o'zgarish: GET metodiga yo'naltirish
                // ******************************************************************
                return RedirectToAction(nameof(ScheduleSetup));
            }
            catch (Exception ex)
            {
                // Xato xabarini TempData orqali saqlaymiz
                TempData["ErrorMessage"] = $"❌ Произошла ошибка при сохранении: {ex.Message}";

                // Xato bo'lsa ham GET metodiga yo'naltirish, lekin ba'zida POST so'rovda qolish maqbulroq.
                // Hozircha GET ga yo'naltiramiz.
                return RedirectToAction(nameof(ScheduleSetup));
            }
        }
    }
}