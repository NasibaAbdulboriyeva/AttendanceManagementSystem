using AttendanceManagementSystem.Api.Configurations;
using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Application.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AttendanceManagementSystem.Infrastructure.Persistence.Gateway
{
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
                _logger.LogCritical("TTLockApiSettings: BaseUrl не найден в конфигурации.");
                throw new InvalidOperationException("Базовый URL TTLock не найден в конфигурации.");
            }
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        }

        public async Task<ICollection<TTLockRecordDto>> GetAllAttendanceLockRecordsAsync(long startDate,long endDate,int? recordType = null)
        {
            var allRecords = new List<TTLockRecordDto>();
            int pageNo = 1;
            int totalPages = 1;

            _logger.LogInformation("Запуск получения логов TTLock. StartDate: {Start}", startDate);

            try
            {
                do
                {
                    var response = await GetLockRecordsPageAsync(
                        startDate, endDate, pageNo, DefaultPageSize, recordType);

                    if (response == null || response.List == null)
                    {
                        _logger.LogWarning("TTLock: Страница {PageNo} не получена. Завершено неудачно.", pageNo);
                        break;
                    }

                    allRecords.AddRange(response.List);
                    totalPages = response.Pages;
                    pageNo++;

                    _logger.LogDebug("Получена страница {PageNo} из {TotalPages} от TTLock. Количество записей: {Count}",  pageNo - 1, totalPages, allRecords.Count);

                    await Task.Delay(50);
                } while (pageNo <= totalPages);

                _logger.LogInformation("От TTLock успешно получено всего {Count} записей.",  allRecords.Count);
                return allRecords;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении логов из TTLock API:{Message}", ex.Message);
                return allRecords;
            }
        }

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
                    return await response.Content.ReadFromJsonAsync<TTLockResponse>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Ошибка TTLock API на странице {PageNo}: Статус {Status}. Содержание: {Content}", pageNo, response.StatusCode, errorContent);

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непредвиденная ошибка в HTTP-запросе: {Message}", ex.Message);
                return null;
            }
        }

     
        public async Task<ICollection<TTLockIcCardDto>> GetAllIcCardRecordsAsync( string? searchStr = null, int orderBy = 1)
        {
            const string endpoint = "identityCard/list";
            var allRecords = new List<TTLockIcCardDto>();
            int pageNo = 1;
            int totalPages = 1;
            const int pageSize = DefaultPageSize;

            _logger.LogInformation("Начата загрузка данных IC-карт TTLock. ");

            while (pageNo <= totalPages)
            {
                long dateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

          
                var url = $"https://euapi.ttlock.com/v3/" + $"{endpoint}?" +
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
                    var response = await _httpClient.GetFromJsonAsync<TTLockGenericResponse<TTLockIcCardDto>>(url);

                    if (response?.List != null && response.List.Any())
                    {
                        allRecords.AddRange(response.List);
                        totalPages = response.Pages;
                        _logger.LogDebug("Страница IC-карт {PageNo}/{TotalPages} загружена. Добавлено {Count} записей.", pageNo, totalPages, response.List.Count);
                        pageNo++;
                        await Task.Delay(50);
                    }
                    else if (pageNo == 1 && (response?.Total ?? 0) == 0)
                    {
                        _logger.LogInformation("Данные для IC-карт не найдены.");
                        break;
                    }
                    else
                    {
                        _logger.LogWarning("Hепредвиденная ситуация при вызове TTLock IC Card API. Страница {PageNo}.", pageNo);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Произошла ошибка при вызове TTLock IC Card API (Страница {PageNo})", pageNo);
                    break;
                }
            }

            _logger.LogInformation("Всего получено {Count} записей для TTLock IC-карт.", allRecords.Count);
            return allRecords;
        }
      
        public async Task<ICollection<TTLockFingerprintDto>> GetAllFingerprintsPaginatedAsync(
            string? searchStr = null, int orderBy = 1)
        {
            const string endpoint = "fingerprint/list";
            var allRecords = new List<TTLockFingerprintDto>();
            int pageNo = 1;
            int totalPages = 1;
            const int pageSize = DefaultPageSize;

            _logger.LogInformation("Старт загрузки данных отпечатков TTLock. ");

            while (pageNo <= totalPages)
            {
                long dateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var url = $"https://euapi.ttlock.com/v3/" + 
                          $"{endpoint}?" +
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
                    var response = await _httpClient.GetFromJsonAsync<TTLockGenericResponse<TTLockFingerprintDto>>(url);

                    if (response?.List != null && response.List.Any())
                    {
                        allRecords.AddRange(response.List);
                        totalPages = response.Pages;
                        _logger.LogDebug("Страница отпечатков {PageNo}/{TotalPages} загружена. Добавлено {Count} записей.", pageNo, totalPages, response.List.Count);
                        pageNo++;
                        await Task.Delay(50);
                    }
                    else if (pageNo == 1 && (response?.Total ?? 0) == 0)
                    {
                        _logger.LogInformation("Данные для отпечатков пальцев не найдены.");
                        break;
                    }
                    else
                    {
                        _logger.LogWarning("Непредвиденная ситуация при вызове Fingerprint API. Страница {PageNo}.", pageNo);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Произошла ошибка при вызове TTLock Fingerprint API (Страница {PageNo})", pageNo);
                    break;
                }
            }

            _logger.LogInformation("Всего получено {Count} записей для отпечатков пальцев TTLock.", allRecords.Count);
            return allRecords;
        }
    }
}