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
        private readonly ITTlockTokenRepository _tokenRepository;
        private readonly ILogger<TTLockService> _logger;
        private const int DefaultPageSize = 200;
        private static readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);
        public TTLockService(IOptions<TTLockSettings> settings, HttpClient httpClient, ILogger<TTLockService> logger, ITTlockTokenRepository tokenRepository)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;
            _tokenRepository = tokenRepository;

            if (string.IsNullOrEmpty(_settings.BaseUrl))
            {
                _logger.LogCritical("TTLockApiSettings: BaseUrl не найден в конфигурации.");
                throw new InvalidOperationException("Базовый URL TTLock не найден в конфигурации.");
            }
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        }

        public async Task<ICollection<TTLockRecordDto>> GetAllAttendanceLockRecordsAsync(long startDate, long endDate, int? recordType = null)
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

                    _logger.LogDebug("Получена страница {PageNo} из {TotalPages} от TTLock. Количество записей: {Count}", pageNo - 1, totalPages, allRecords.Count);

                    await Task.Delay(50);
                } while (pageNo <= totalPages);

                _logger.LogInformation("От TTLock успешно получено всего {Count} записей.", allRecords.Count);
                return allRecords;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при получении логов из TTLock API:{Message}", ex.Message);
                return allRecords;
            }
        }

        public async Task InitializeTokensFromConfigAsync()
        {
            _logger.LogInformation("Начало инициализации токенов из конфигурации.");

            var initialToken = new TTLockSettings
            {
                Id = 1,
                ClientId = _settings.ClientId,
                ClientSecret = _settings.ClientSecret,
                BaseUrl = _settings.BaseUrl,
                LockId = _settings.LockId,
                AccessToken = _settings.AccessToken,
                RefreshToken = _settings.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-5)
            };

            await _tokenRepository.InitializeTokenAsync(initialToken);
            _logger.LogInformation("Инициализация токенов TTLock завершена.");
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var tokenRecord = await _tokenRepository.GetTokenRecordAsync();

            if (tokenRecord == null)
            {
                throw new InvalidOperationException("TTLock токен не инициализирован.");
            }

            bool isTokenExpiredOrAboutToExpire =
                tokenRecord.ExpiresAt.ToUniversalTime() < DateTime.UtcNow.AddMinutes(15);

            if (!isTokenExpiredOrAboutToExpire)
            {
                return tokenRecord.AccessToken;
            }

            _logger.LogWarning("TTLock Access Token истек или скоро истечет. Попытка обновления...");
            return await RefreshAndSaveTokenAsync(tokenRecord);
        }

        // 1.3. Tokenni yangilash (Refresh) mantiqi
        private async Task<string> RefreshAndSaveTokenAsync(TTLockSettings oldRecord)
        {
            await _refreshLock.WaitAsync();
            try
            {

                var latestTokenRecord = await _tokenRepository.GetTokenRecordAsync();
                if (latestTokenRecord.ExpiresAt.ToUniversalTime() > DateTime.UtcNow.AddMinutes(5))
                {
                    return latestTokenRecord.AccessToken;
                }

                // 2. Refresh API chaqiruvi (TTLock serveriga)
                var newTokens = await CallTtlockRefreshApiAsync(latestTokenRecord.RefreshToken);

                if (newTokens.access_token != null && newTokens.refresh_token != null)
                {
                    latestTokenRecord.AccessToken = newTokens.access_token;
                    latestTokenRecord.RefreshToken = newTokens.refresh_token;
                    latestTokenRecord.ExpiresAt = DateTime.UtcNow.AddSeconds(newTokens.expires_in);

                    await _tokenRepository.UpdateTokenAsync(latestTokenRecord);
                    _logger.LogInformation("TTLock Access Token успешно обновлен.");

                }
                return latestTokenRecord.AccessToken;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка при обновлении TTLock токена.");
                throw new InvalidOperationException("Не удалось обновить TTLock токен.", ex);
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        // 1.4. Refresh API chaqiruvi uchun yordamchi metod (ITTlockRefreshApiService dan olingan)
        private async Task<TTLockApiTokenResponse> CallTtlockRefreshApiAsync(string currentRefreshToken)
        {
            var url = "https://euapi.ttlock.com/oauth2/token";

            // So'rov rasmga (curl) mos kelishi uchun x-www-form-urlencoded formatida tayyorlanadi.
            var formContent = new FormUrlEncodedContent(new[]
            {
        // Rasmda ko'rsatilgan barcha maydonlar
             new KeyValuePair<string, string>("clientId", _settings.ClientId),
             new KeyValuePair<string, string>("clientSecret", _settings.ClientSecret),
             new KeyValuePair<string, string>("grant_type", "refresh_token"),
             new KeyValuePair<string, string>("refresh_token", currentRefreshToken)
            });

            // 1. So'rovni yuborish
            var response = await _httpClient.PostAsync(url, formContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Ошибка HTTP при обновлении токена: {response.StatusCode}. Ответ: {errorContent}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TTLockApiTokenResponse>();

            if (tokenResponse.errcode != 0)
            {
                throw new Exception($"TTLock Refresh API ошибка: {tokenResponse.errmsg} (Код: {tokenResponse.errcode})");
            }

            return tokenResponse;
        }


        private async Task<TTLockResponse?> GetLockRecordsPageAsync(long startDate, long endDate, int pageNo, int pageSize, int? recordType)
        {
            long currentDateMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var accessToken = await GetAccessTokenAsync();
            var url = $"v3/lockRecord/list?" +
                        $"clientId={_settings.ClientId}&" +
                        $"accessToken={accessToken}&" +
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


        public async Task<ICollection<TTLockIcCardDto>> GetAllIcCardRecordsAsync(string? searchStr = null, int orderBy = 1)
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
                var accessToken = await GetAccessTokenAsync();


                var url = $"https://euapi.ttlock.com/v3/" + $"{endpoint}?" +
                          $"clientId={_settings.ClientId}&" +
                          $"accessToken={accessToken}&" +
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
                var accessToken = await GetAccessTokenAsync();

                var url = $"https://euapi.ttlock.com/v3/" +
                          $"{endpoint}?" +
                          $"clientId={_settings.ClientId}&" +
                          $"accessToken={accessToken}&" +
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