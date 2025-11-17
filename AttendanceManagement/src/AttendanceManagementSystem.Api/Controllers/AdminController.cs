using AttendanceManagementSystem.Application.DTOs;
using AttendanceManagementSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;

public class AdminController : Controller
{
    private readonly IUploadService _uploadService;

    public AdminController(IUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    public ActionResult Index()
    {
        return View();
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> UploadAttendanceLogs(UploadRequestDto request)
    {
        if (request == null || request.File == null || request.File.Length == 0)
        {
            ViewBag.ErrorMessage = "Iltimos, yuklash uchun faylni tanlang.";
            return View();
        }

        var fileName = request.File.FileName;

        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
            !fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.ErrorMessage = "Faqat .xlsx yoki .xls formatidagi fayllarga ruxsat beriladi.";
            return View();
        }

        try
        {
            var logCount = await _uploadService.UploadAttendanceLogAsync(request);

            ViewBag.SuccessMessage =
                $"Davomat loglari saqlandi. Yozuvlar soni: {logCount}";
        }
        catch (ArgumentException ex)
        {
            ViewBag.ErrorMessage = ex.Message;
        }
        catch
        {
            ViewBag.ErrorMessage = "Kutilmagan xato yuz berdi.";
        }

        return View();
    }
}
