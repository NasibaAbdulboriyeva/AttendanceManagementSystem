using AttendanceManagementSystem.Api.Configurations;
using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AttendanceManagementSystem.Infrastructure.Persistence.Gateway
{
    // Eslatma: TTLockSettings, TTLockGenericResponse<T>, TTLockRecordDto va TTLockResponse (TTLockGenericResponse<TTLockRecordDto>) 
    // DTO/Modelarining mavjudligi va to'g'ri ishlashi talab qilinadi.

    public class TTLockService : ITTLockService
    {
        private readonly TTLockSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TTLockService> _logger;
        private const int DefaultPageSize = 200;

        public TTLockService(
            IOptions<TTLockSettings> settings,
            HttpClient httpClient,
            ILogger<TTLockService> logger)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;

            if (string.IsNullOrEmpty(_settings.BaseUrl))
            {
                _logger.LogCritical("TTLockApiSettings: BaseUrl konfiguratsiyada topilmadi.");
                throw new InvalidOperationException("TTLock Base URL konfiguratsiyada mavjud emas.");
            }
            // BaseAddress o'rnatish
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        }

        // --- 1. Lock Recordlarini Sahifalash Mantig'i (O'zgartirilmadi) ---
        // Bu metodda pagination mantig'ini olib tashlash so'ralmagan, shuning uchun u qoladi.
        public async Task<ICollection<TTLockRecordDto>> GetAllAttendanceLockRecordsAsync(
            long startDate,
            long endDate,
            int? recordType = null)
        {
            var allRecords = new List<TTLockRecordDto>();
            int pageNo = 1;
            int totalPages = 1;

            _logger.LogInformation("TTLock loglarni olish boshlandi. StartDate: {Start}", startDate);

            try
            {
                do
                {
                    var response = await GetLockRecordsPageAsync(
                        startDate, endDate, pageNo, DefaultPageSize, recordType);

                    if (response == null || response.List == null)
                    {
                        _logger.LogWarning("TTLock:  sahifa {PageNo} olinmadi. Muvaffaqiyatsiz yakunlandi.", pageNo);
                        break;
                    }

                    allRecords.AddRange(response.List);
                    totalPages = response.Pages;
                    pageNo++;

                    _logger.LogDebug("TTLockdan {PageNo}/{TotalPages} sahifa olindi. Yozuvlar soni: {Count}",  pageNo - 1, totalPages, allRecords.Count);

                    await Task.Delay(50);
                } while (pageNo <= totalPages);

                _logger.LogInformation("TTLockdan jami {Count} ta yozuv muvaffaqiyatli olindi.",  allRecords.Count);
                return allRecords;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TTLock API'dan loglarni olishda xato yuz berdi: {Message}", ex.Message);
                return allRecords;
            }
        }

        // --- 2. Lock Record Bir Sahifasini Olish (O'zgartirilmadi) ---
        private async Task<TTLockResponse?> GetLockRecordsPageAsync(long startDate, long endDate, int pageNo,int pageSize, int? recordType)
        {
            long currentDateMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var url = $"v3/lockRecord/list?" +
                        $"clientId={_settings.ClientId}&" +
                        $"accessToken={_settings.AccessToken}&" +
                        $"lockId={_settings.LockId}&" +
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
                    // TTLockResponse turi TTLockGenericResponse<TTLockRecordDto> bo'lishi kerak.
                    return await response.Content.ReadFromJsonAsync<TTLockResponse>();
                }

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

        // ---------------------------------------------------------------------
        // ## 💳 IC Card Records uchun Pagination (To'liq Qayta Yozilgan)
        // ---------------------------------------------------------------------
        public async Task<ICollection<TTLockIcCardDto>> GetAllIcCardRecordsAsync( string? searchStr = null, int orderBy = 1)
        {
            const string endpoint = "identityCard/list";
            var allRecords = new List<TTLockIcCardDto>();
            int pageNo = 1;
            int totalPages = 1;
            const int pageSize = DefaultPageSize;

            _logger.LogInformation("TTLock IC Card ma'lumotlarini yuklash boshlandi ");

            while (pageNo <= totalPages)
            {
                long dateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                // URL tuzish
                var url = $"{endpoint}?" +
                          $"clientId={_settings.ClientId}&" +
                          $"accessToken={_settings.AccessToken}&" +
                          $"lockId={_settings.LockId}&" +
                          $"pageNo={pageNo}&" +
                          $"pageSize={pageSize}&" +
                          $"orderBy={orderBy}&" +
                          $"date={dateTimestamp}";

                if (!string.IsNullOrEmpty(searchStr))
                {
                    url += $"&searchStr={Uri.EscapeDataString(searchStr)}";
                }

                try
                {
                    // TTLockGenericResponse<TTLockIcCardDto> turiga deserialize qilish
                    var response = await _httpClient.GetFromJsonAsync<TTLockGenericResponse<TTLockIcCardDto>>(url);

                    if (response?.List != null && response.List.Any())
                    {
                        allRecords.AddRange(response.List);
                        totalPages = response.Pages;
                        _logger.LogDebug("IC Card sahifasi {PageNo}/{TotalPages} yuklandi. {Count} ta yozuv qo'shildi.", pageNo, totalPages, response.List.Count);
                        pageNo++;
                        await Task.Delay(50);
                    }
                    else if (pageNo == 1 && (response?.Total ?? 0) == 0)
                    {
                        _logger.LogInformation("IC Card uchun ma'lumotlar topilmadi.");
                        break;
                    }
                    else
                    {
                        _logger.LogWarning("IC Card API chaqiruvida kutilmagan holat. Sahifa {PageNo}.", pageNo);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "TTLock IC Card API chaqiruvida xato yuz berdi (Page {PageNo})", pageNo);
                    break;
                }
            }

            _logger.LogInformation("TTLock IC Card uchun jami {Count} ta yozuv olindi.", allRecords.Count);
            return allRecords;
        }

        // ---------------------------------------------------------------------
        // ## 👆 Fingerprint Records uchun Pagination (To'liq Qayta Yozilgan)
        // ---------------------------------------------------------------------
        public async Task<ICollection<TTLockFingerprintDto>> GetAllFingerprintsPaginatedAsync(
            string? searchStr = null, int orderBy = 1)
        {
            const string endpoint = "fingerprint/list";
            var allRecords = new List<TTLockFingerprintDto>();
            int pageNo = 1;
            int totalPages = 1;
            const int pageSize = DefaultPageSize;

            _logger.LogInformation("TTLock Fingerprint ma'lumotlarini yuklash boshlandi ");

            while (pageNo <= totalPages)
            {
                long dateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                // URL tuzish
                var url = $"{endpoint}?" +
                          $"clientId={_settings.ClientId}&" +
                          $"accessToken={_settings.AccessToken}&" +
                          $"lockId={_settings.LockId}&" +
                          $"pageNo={pageNo}&" +
                          $"pageSize={pageSize}&" +
                          $"orderBy={orderBy}&" +
                          $"date={dateTimestamp}";

                if (!string.IsNullOrEmpty(searchStr))
                {
                    url += $"&searchStr={Uri.EscapeDataString(searchStr)}";
                }

                try
                {
                    // TTLockGenericResponse<TTLockFingerprintDto> turiga deserialize qilish
                    var response = await _httpClient.GetFromJsonAsync<TTLockGenericResponse<TTLockFingerprintDto>>(url);

                    if (response?.List != null && response.List.Any())
                    {
                        allRecords.AddRange(response.List);
                        totalPages = response.Pages;
                        _logger.LogDebug("Fingerprint sahifasi {PageNo}/{TotalPages} yuklandi. {Count} ta yozuv qo'shildi.", pageNo, totalPages, response.List.Count);
                        pageNo++;
                        await Task.Delay(50);
                    }
                    else if (pageNo == 1 && (response?.Total ?? 0) == 0)
                    {
                        _logger.LogInformation("Fingerprint uchun ma'lumotlar topilmadi.");
                        break;
                    }
                    else
                    {
                        _logger.LogWarning("Fingerprint API chaqiruvida kutilmagan holat. Sahifa {PageNo}.", pageNo);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "TTLock Fingerprint API chaqiruvida xato yuz berdi (Page {PageNo})", pageNo);
                    break;
                }
            }

            _logger.LogInformation("TTLock Fingerprint uchun jami {Count} ta yozuv olindi.", allRecords.Count);
            return allRecords;
        }
    }
}