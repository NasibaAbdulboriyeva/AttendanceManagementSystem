using AttendanceManagementSystem.Api.Models;
using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs;
using AttendanceManagementSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AttendanceManagementSystem.Api.Controllers
{
    public class AdminAttendanceController : Controller
    {
        private readonly IAttendanceLogService _logService;
        private readonly IEmployeeService _employeeService;
        private readonly ICurrentAttendanceLogCalculationService _calculationService;
        private readonly AttendanceSettings _settings;
        public AdminAttendanceController(IAttendanceLogService logService, IEmployeeService employeeService, ICurrentAttendanceLogCalculationService calculationService, IOptions<AttendanceSettings> options)
        {
            _logService = logService;
            _employeeService = employeeService;
            _calculationService = calculationService;
            _settings = options.Value;

        }
        [HttpGet]
        public IActionResult Instructions()
        {
           
            ViewData["Title"] = "Инструкция по Использованию Админ-Панели";
            return View();
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
        public async Task<IActionResult> GetDetails(string username, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var employeeId = await _employeeService.GetEmployeeIdByUsernameAsync(username);
            var logs = await _logService.GetLogsByEmployeeIdAsync(employeeId, startDate, endDate);
            var result = logs.Select(log => new AttendanceLogModel
            {
                EntryTime = log.RecordedTime,
                EmployeeName = username,

            }).ToList();

            return View("AllAttendanceLogs", result);
        }
        [HttpGet]
        public async Task<IActionResult> GetDailyRawLogs(long employeeId, string date)
        {
            DateTime targetDate = DateTime.Parse(date);

          
            var logs = await _logService.GetDailyRawLogsAsync(employeeId, targetDate);

            var result = logs.OrderBy(l => l.RecordedTime).Select(l => new {
                RecordedTime = l.RecordedTime.ToString("HH:mm:ss")
            });

            return Json(result);
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
        public async Task<IActionResult> SyncCalendarForAll(int year, int month)
        {
            var targetMonth = new DateTime(year == 0 ? DateTime.Now.Year : year, month == 0 ? DateTime.Now.Month : month, 1);

            var targetMonthDateOnly = DateOnly.FromDateTime(targetMonth);
            bool alreadyRunThisMonth = await _calculationService.HasMonthlyAttendanceLogs(targetMonth);
            var finalViewModel = new AttendanceCalendarViewModel();
            if (!alreadyRunThisMonth)
            {

                try
                {
                    await _calculationService.ProcessAllEmployeesMonthlyAttendanceAsync(targetMonth);

                    alreadyRunThisMonth = await _calculationService.HasMonthlyAttendanceLogs(targetMonth);

                    if (alreadyRunThisMonth)
                    {

                        finalViewModel = new AttendanceCalendarViewModel 
                        {
                            TargetMonth = targetMonth,
                            
                        };
                    }
                    else
                    {

                        TempData["ErrorMessage"] = $"⚠️ Создание календаря за {targetMonth:yyyy-MM} месяц завершено, но записи (логи) не были найдены.";
                    }

                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"❌ Ошибка при создании календаря за {targetMonth:yyyy-MM}: {ex.Message}";
                }
            }
            else
            {
                finalViewModel = new AttendanceCalendarViewModel
                {
                    TargetMonth = targetMonth,
                    DefaultLimit = _settings.DefaultMonthlyLimit

                };
                await _calculationService.ProcessUpdateForAllEmployeesMonthlyAttendanceAsync(targetMonthDateOnly);
            }

            return View("Calendar",finalViewModel);

        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> LateArrivalsSummary(DateTime? targetMonth)
        {
            var month = targetMonth.HasValue
                ? new DateTime(targetMonth.Value.Year, targetMonth.Value.Month, 1)
                : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

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
                TargetMonth = month,
                MonthlyLimit = _settings.DefaultMonthlyLimit    
            };

            return View(model);
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> DailyLateArrivalsSummary(DateOnly? day)
        {
            
            DateOnly selectedDay = day ?? DateOnly.FromDateTime(DateTime.Now);

            
            var allEmployeesRaw = await _employeeService.GetAllActiveEmployeesAsync();

            var lateSummaryDictionary = await _calculationService.GetEmployeesDailyLateSummaryAsync(selectedDay);

            var employeesSummary = allEmployeesRaw
                .Select(e => new EmployeeSummary
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.UserName,
                    TotalLateMinutes = lateSummaryDictionary.TryGetValue(e.EmployeeId, out int totalMinutes)
                                       ? totalMinutes
                                       : 0
                })
                .ToList();

            var model = new AttendanceSummaryViewModel
            {
                Employees = employeesSummary,
          
                TargetMonth = selectedDay.ToDateTime(TimeOnly.MinValue)
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
                month == 0 ? DateTime.Now.Month : month, 1);

            var employeeId = await _employeeService.GetEmployeeIdByUsernameAsync(username);
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);

            var logs = await _calculationService.GetMonthlyAttendanceCalendarsAsync(employeeId, targetMonth);

            var viewModel = new AttendanceCalendarViewModel
            {
                TargetMonth = targetMonth,
                EmployeeFullName = employee.UserName,
                MonthlyLogs = logs,
                DefaultLimit= _settings.DefaultMonthlyLimit,
                TotalUnjustifiedLates = await _calculationService.GetRemainingMonthlyLimitAsync(employeeId, DateOnly.FromDateTime(targetMonth))
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
            var allSchedules = await _employeeService.GetAllSchedulesAsync();

            var scheduleDict = allSchedules
                .GroupBy(s => s.EmployeeId)
                .ToDictionary(g => g.Key, g => g.First());

            var viewModel = new EmployeeScheduleViewModel
            {
                Message = TempData["SuccessMessage"]?.ToString() ?? TempData["ErrorMessage"]?.ToString(),

                Schedules = allEmployeesDto.OrderBy(e => e.UserName).Select(e =>
                {
                    scheduleDict.TryGetValue(e.EmployeeId, out var s);
                    return new ScheduleListItem
                    {
                        FullName = e.UserName,
                        EmployeeScheduleId = s?.EmployeeScheduleId ?? 0,
                        StartTime = s?.StartTime ?? new TimeOnly(9, 0), 
                        EndTime = s?.EndTime ?? new TimeOnly(18, 0),   
                        LimitInMinutes = s?.LimitInMinutes ?? 0,
                        IsSelected = false
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]

        public async Task<IActionResult> UpdateEntryTime([FromBody] UpdateEntryTimeDto dto)
        {

            var newLateMinutes = await _calculationService.UpdateEntryTimeManuallyAsync(dto);

            return Ok(new
            {
                success = true,
                lateMinutes = newLateMinutes
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
        [HttpPost]
        public async Task<IActionResult> UpdateDescription([FromBody] DescriptionUpdateDto dto)
        {
            if (dto == null || dto.EmployeeId <= 0 || dto.EntryDay == default)
            {
                return BadRequest(new { isSuccess = false, message = "Некорректные данные запроса (необходимы ID сотрудника и дата)" });
            }

            try
            {

                var updatedDescription = await _calculationService.UpdateDescriptionAsync(dto);

                if (updatedDescription != null)
                {

                    return Ok(new
                    {
                        isSuccess = true,
                        message = "Комментарий  был успешно обновлен..",
                        newDescription = updatedDescription
                    });
                }
                else
                {
                    return NotFound(new
                    {
                        isSuccess = false,
                        message = $"Запись для сотрудника с ID: {dto.EmployeeId} на дату {dto.EntryDay.ToShortDateString()} не найдена."
                    });
                }
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, new
                {
                    isSuccess = false,
                    message = $"Внутренняя ошибка сервера: {ex.Message}"
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
            var selectedSchedules = model.Schedules.Where(x => x.IsSelected).ToList();

            if (!selectedSchedules.Any())
            {
                model.Message = "❌ Пожалуйста, выберите хотя бы одного сотрудника для обновления.";
                return View(model);
            }
            try
            {
                foreach (var item in selectedSchedules)
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
                    if (schedule==null|| schedule.EmployeeId == 0)
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