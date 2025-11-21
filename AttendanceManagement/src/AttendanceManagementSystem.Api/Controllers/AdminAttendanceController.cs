using Microsoft.AspNetCore.Mvc;
using AttendanceManagementSystem.Application.Services;
using AttendanceManagementSystem.Api.Models; // Modelga murojaat

namespace AttendanceManagementSystem.Web.Controllers // Sizning asosiy namespace'ingiz
{
    // Eslatma: HomeControllerdan farqlanishi uchun AdminAttendanceController deb nomladik
    public class AdminAttendanceController : Controller
    {
        // Service ni o'zgaruvchi sifatida e'lon qilamiz
        private readonly IAttendanceLogService _logService;

        // Constructor orqali Service ni Inject (DI) qilamiz
        public AdminAttendanceController(IAttendanceLogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new SyncViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Xavfsizlik uchun tavsiya etiladi
        public async Task<IActionResult> Index(SyncViewModel model)
        {
            // Validatsiya (ID kiritilganmi, to'g'rimi?)
            if (!ModelState.IsValid)
            {
                // Agar validatsiya o'tmasa, View ni xatolar bilan qaytaramiz
                return View(model);
            }

            try
            {
                // To'g'ridan-to'g'ri service metodini chaqiramiz (Bazaga saqlash shu yerda sodir bo'ladi)
                int savedCount = await _logService.SyncAttendanceLogsAsync(model.LockId, null, null);

                // Natijani modelga yozamiz
                model.SyncedCount = savedCount;
                model.Message = $"✅ Muvaffaqiyat! {savedCount} ta yangi log bazaga saqlandi.";
            }
            catch (Exception ex)
            {
                // Xato yuz bersa
                model.Message = $"❌ Xatolik yuz berdi: {ex.Message}";
                // Katta xatolikni loglarga yozish tavsiya etiladi
            }

            // Xabar bilan View ni qayta ko'rsatamiz
            return View(model);
        }
    }
}