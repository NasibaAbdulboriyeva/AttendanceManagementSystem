using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AttendanceManagementSystem.Infrastructure.Persistence.Gateway
{
    // Eslatma: TTlockApiSettings kabi modelni ishlatish uchun TTLockSettings nomini TTlockApiSettings deb o'zgartiraman.
    public class TTLockService : ITTLockService
    {
        private readonly TTLockSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TTLockService> _logger;

        public TTLockService(
            IOptions<TTLockSettings> settings,
            HttpClient httpClient,
            ILogger<TTLockService> logger)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;

            // Base URL ni tekshirish va o'rnatish
            if (string.IsNullOrEmpty(_settings.BaseUrl))
            {
                _logger.LogCritical("TTLockApiSettings: BaseUrl konfiguratsiyada topilmadi.");
                // Asosiy xatolikni otamiz (throw)
                throw new InvalidOperationException("TTLock Base URL konfiguratsiyada mavjud emas.");
            }
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        }

        // --- 1. ATTENDANCE LOGLARINI OLISH (Barcha sahifalar) ---

        public async Task<ICollection<TTLockRecordDto>> GetAllAttendanceLockRecordsAsync(
            int lockId,
            long startDate,
            long endDate,
            int? recordType = null)
        {
            var allRecords = new List<TTLockRecordDto>();
            int pageNo = 1;
            int totalPages = 1;
            const int pageSize = 200; // TTLock maksimal sahifa hajmi

            _logger.LogInformation("TTLock: {LockId} uchun loglarni olish boshlandi. StartDate: {Start}", lockId, startDate);

            try
            {
                do
                {
                    // Ichki yordamchi metod yordamida bir sahifani yuklaymiz
                    var response = await GetLockRecordsPageAsync(
                        lockId, startDate, endDate, pageNo, pageSize, recordType);

                    if (response == null || response.List==null )
                    {
                        // Xato loglangan bo'lsa, siklni to'xtatamiz
                        _logger.LogWarning("TTLock: {LockId} dan sahifa {PageNo} olinmadi. Muvaffaqiyatsiz yakunlandi.", lockId, pageNo);
                        break;
                    }

                    // Ma'lumotlarni yig'ish va sahifalash qiymatlarini yangilash
                    allRecords.AddRange(response.List);
                    totalPages = response.Pages;
                    pageNo++;

                    _logger.LogDebug("TTLock: {LockId} dan {PageNo}/{TotalPages} sahifa olindi. Yozuvlar soni: {Count}", lockId, pageNo - 1, totalPages, allRecords.Count);

                    // API serveriga yuklamani kamaytirish uchun qisqa kutish
                    await Task.Delay(50);

                } while (pageNo <= totalPages);

                _logger.LogInformation("TTLock: {LockId} dan jami {Count} ta yozuv muvaffaqiyatli olindi.", lockId, allRecords.Count);
                return allRecords;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TTLock API'dan loglarni olishda xato yuz berdi: {Message}", ex.Message);
                
                return allRecords;
            }
        }

  

        private async Task<TTLockResponse> GetLockRecordsPageAsync(
            int lockId,
            long startDate,
            long endDate,
            int pageNo,
            int pageSize,
            int? recordType)
        {
            long currentDateMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // 1. So'rov URL'ini tuzish (Global tokenlardan foydalangan holda)
            var url = $"v3/lockRecord/list?" +
                      $"clientId={_settings.ClientId}&" +
                      $"accessToken={_settings.AccessToken}&" + // ⚠️ Eslatma: Token muddati tugagan bo'lsa, bu yerda xato bo'ladi.
                      $"lockId={lockId}&" +
                      $"startDate={startDate}&" +
                      $"endDate={endDate}&" +
                      $"pageNo={pageNo}&" +
                      $"pageSize={pageSize}&" +
                      $"date={currentDateMs}";

            if (recordType.HasValue)
            {
                url += $"&recordType={recordType.Value}";
            }

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    // JSON javobini TTLockResponse (Pagination DTO) modeliga o'girish
                    return await response.Content.ReadFromJsonAsync<TTLockResponse>();
                }

                // Agar status kod muvaffaqiyatli bo'lmasa (401, 500, va h.k.)
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("TTLock API Error Page {PageNo}: Status {Status}. Content: {Content}", pageNo, response.StatusCode, errorContent);

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP so'rovida kutilmagan xato: {Message}", ex.Message);
                return null;
            }
        }

        // --- QOLGAN METODLAR (Yangi interfeysga moslab o'zgartirilgan) ---

        public Task<ICollection<TTLockIcCardDto>> GetAllIcCardRecordsAsync(int lockId, string? searchStr = null, int orderBy = 1)
        {
            // Bu yerda ham GetAllAttendanceLockRecordsAsync kabi Pagination mantig'i bo'ladi.
            throw new NotImplementedException();
        }

        public Task<ICollection<TTLockFingerprintDto>> GetAllFingerprintsPaginatedAsync(int lockId, string? searchStr = null, int orderBy = 1)
        {
            // Bu yerda ham Pagination mantig'i bo'ladi.
            throw new NotImplementedException();
        }

      
    }
}