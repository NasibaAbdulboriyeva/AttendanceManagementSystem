using Microsoft.AspNetCore.Mvc;
using AttendanceManagementSystem.Application.Abstractions; // IEmployeeService shu yerda bo'lishi kerak
using AttendanceManagementSystem.Api.Models; // SyncViewModel shu yerda bo'lishi kerak

using AttendanceManagementSystem.Application.Services;

namespace AttendanceManagementSystem.Web.Controllers
{
    // Eslatma: HomeControllerdan farqlanishi uchun AdminAttendanceController deb nomladik
    public class AdminAttendanceController : Controller
    {
        // Service'lar ni o'zgaruvchi sifatida e'lon qilamiz
        private readonly IAttendanceLogService _logService;
        private readonly IEmployeeService _employeeService; // ✨ EMPLOYEE SERVICE QO'SHILDI

        // Constructor orqali Service'lar ni Inject (DI) qilamiz
        public AdminAttendanceController(
            IAttendanceLogService logService,
            IEmployeeService employeeService) // ✨ EMPLOYEE SERVICE QO'SHILDI
        {
            _logService = logService;
            _employeeService = employeeService;
        }

        // ---------------------------------------------------------------------
        // 1. Loglarni Sinxronlash View'i (Index)
        // ---------------------------------------------------------------------

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
                // LockId'ni Model.LockId o'rniga model.LockId.Value orqali oling, agar u int? bo'lsa.
                // Log service chaqiruvi
                int savedCount = await _logService.SyncAttendanceLogsAsync(model.StartDate,model.EndDate);

                model.SyncedCount = savedCount;
                model.Message = $"✅ Loglar Muvaffaqiyatli! {savedCount} ta yangi log bazaga saqlandi.";
            }
            catch (Exception ex)
            {
                model.Message = $"❌ Log sinxronizatsiyasida Xatolik: {ex.Message}";
                // Logging mexanizmini qo'shing (masalan, ILogger orqali)
            }

            return View(model);
        }

        // ---------------------------------------------------------------------
        // 2. Xodimlarni Sinxronlash Action'i (EmployeeSync)
        // ---------------------------------------------------------------------

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
                model.Message = $"✅ Xodimlar Muvaffaqiyatli Sinxronlandi! " +
                                $"Jami {model.SyncedCount} ta yozuv (Card: {cardsSynced}, Fingerprint: {fingerprintsSynced}) yangilandi/qo'shildi.";
            }
            catch (Exception ex)
            {
                // Xato yuz bersa
                model.Message = $"❌ Xodimlar sinxronizatsiyasida Xatolik yuz berdi: {ex.Message}";
                // Logging mexanizmini qo'shish tavsiya etiladi.
            }

            // Xabar bilan View ni qayta ko'rsatamiz
            return View(model);
        }
    }
}