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
        public async Task<IActionResult> Employees()
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
        public async Task<IActionResult> ViewUpdateCalendarForAll(int year, int month)
        {
            
            var targetMonth = new DateOnly(
                year == 0 ? DateTime.Now.Year : year, 
                month == 0 ? 11 : month,             
                1                                   
            );

            await _calculationService.ProcessUpdateForAllEmployeesMonthlyAttendanceAsync(targetMonth);

            var emptyViewModel = new AttendanceCalendarViewModel
            {
                
                TargetMonth = targetMonth.ToDateTime(TimeOnly.MinValue),

            };


            return View("Calendar", emptyViewModel);
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> LateArrivalsSummary(DateTime? targetMonth)
        {

            var month = targetMonth.HasValue
                ? new DateTime(targetMonth.Value.Year, targetMonth.Value.Month, 1)
                : new DateTime(DateTime.Now.Year, 11, 1);

            var allEmployeesRaw = await _employeeService.GetAllActiveEmployeesAsync();

            var employeesSummary = allEmployeesRaw
                .Select(e => new EmployeeSummary
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.UserName,
                    TotalLateMinutes = 0
                })
                .ToList();

            var lateSummaryDictionary = await _calculationService.GetEmployeesLateSummaryAsync(month);

            foreach (var employee in employeesSummary)
            {
                if (lateSummaryDictionary.TryGetValue(employee.EmployeeId, out int totalMinutes))
                {
                    employee.TotalLateMinutes = totalMinutes;

                }

            }

            var model = new AttendanceSummaryViewModel
            {
                Employees = employeesSummary,
                TargetMonth = month
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> ViewCalendar(string username, int year, int month)
        {
            if (username == null)
            {
                return RedirectToAction("EmployeeList");

            }

            var targetMonth = new DateTime(
                year == 0 ? DateTime.Now.Year : year,
                month == 0 ? 11 : month, 1);

            var employeeId = await _employeeService.GetEmployeeIdByUsernameAsync(username);
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);


            var logs = await _calculationService.GetMonthlyAttendanceCalendarsAsync(employeeId, targetMonth);

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

            if (TempData["SuccessMessage"] is string successMessage)
            {
                viewModel.Message = successMessage;
            }
            else if (TempData["ErrorMessage"] is string errorMessage)
            {
                viewModel.Message = errorMessage;
            }

            foreach (var employeeDto in allEmployeesDto.OrderBy(e => e.UserName))
            {

                var scheduleDto = await _employeeService.GetEmployeeScheduleByEmployeeIdAsync(employeeDto.EmployeeId);

                var item = new ScheduleListItem
                {
                    FullName = employeeDto.UserName,

                };

                if (scheduleDto != null)
                {
                    item.EmployeeScheduleId = scheduleDto.EmployeeScheduleId;
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

            var newLateMinutes = await _calculationService.UpdateEntryTimeManuallyAsync(dto);

            return Ok(new
            {
                success = true,
                lateMinutes = newLateMinutes // 👈 YANGI MINUT QO'SHILDI
            });
        }

        [HttpPost]

        public async Task<IActionResult> UpdateJustificationStatus([FromBody] UpdateJustificationDto dto)
        {
            var newLateMinutes = await _calculationService.UpdateJustificationStatusAsync(dto);

            return Ok(new
            {
                success = true,
                lateMinutes = newLateMinutes,
            });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateWorkingDayStatus([FromBody] WorkingDayStatusUpdateDto dto)
        {
            var newLateMinutes = await _calculationService.UpdateWorkingDayStatusAsync(dto);

            return Ok(new
            {
                success = true,
                lateMinutes = newLateMinutes,
            });
        }
        [HttpPost("UpdateDescription")]
        public async Task<IActionResult> UpdateDescription([FromBody] DescriptionUpdateDto dto)
        {
            // Boshlang'ich tekshiruv
            if (dto == null || dto.EmployeeId <= 0 || dto.EntryDay == default)
            {
                return BadRequest(new { isSuccess = false, message = "Noto'g'ri so'rov ma'lumotlari (Xodim ID va Sana zarur)." });
            }

            try
            {
                // Metod nomi Service'dagi yangi nomga mos kelishi kerak.
                var updatedDescription = await _calculationService.UpdateDescriptionAsync(dto);

                if (updatedDescription != null)
                {
                    // Muvaffaqiyatli javob: yangilangan matnni mijozga qaytarish
                    return Ok(new
                    {
                        isSuccess = true,
                        message = "Izoh muvaffaqiyatli yangilandi.",
                        newDescription = updatedDescription
                    });
                }
                else
                {
                    // Log topilmaganda
                    return NotFound(new
                    {
                        isSuccess = false,
                        message = $"Xodim ID: {dto.EmployeeId} uchun {dto.EntryDay.ToShortDateString()} sanasiga mos yozuv topilmadi."
                    });
                }
            }
            catch (Exception ex)
            {
                // Server xatosi
                return StatusCode(500, new
                {
                    isSuccess = false,
                    message = $"Serverda ichki xato: {ex.Message}"
                });
            }
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
                   
                    var emploeeId = await _employeeService.GetEmployeeIdByUsernameAsync(item.FullName);

                    var scheduleDto = new EmployeeScheduleDto
                    {
                       
                        EmployeeId = emploeeId,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        LimitInMinutes = item.LimitInMinutes,
                        EmployementType = item.EmployementType
                    };

                 
                    var schedule = await _employeeService.GetEmployeeScheduleByEmployeeIdAsync(emploeeId);
                    if (schedule.EmployeeScheduleId== 0)
                    {
                        await _employeeService.AddEmployeeScheduleAsync(scheduleDto);
                    }
                    else
                    {
                        
                        await _employeeService.UpdateEmployeeScheduleAsync(scheduleDto);
                    }

                    updatedCount++;
                }

                TempData["SuccessMessage"] = $"✅ Все {updatedCount} расписаний успешно сохранены или обновлены!";

                return RedirectToAction(nameof(ScheduleSetup));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Произошла ошибка при сохранении: {ex.Message}";
                return RedirectToAction(nameof(ScheduleSetup));
            }
        }
    }
}