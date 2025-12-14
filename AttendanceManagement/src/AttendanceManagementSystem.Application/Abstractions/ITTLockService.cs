using AttendanceManagementSystem.Application.DTOs;

namespace AttendanceManagementSystem.Application.Abstractions
{
    public interface ITTLockService
    {
        Task<ICollection<TTLockRecordDto>> GetAllAttendanceLockRecordsAsync(long startDate,long endDate,int? recordType = null);

        Task<ICollection<TTLockIcCardDto>> GetAllIcCardRecordsAsync(string? searchStr = null,int orderBy = 1);

        Task<ICollection<TTLockFingerprintDto>> GetAllFingerprintsPaginatedAsync(string? searchStr = null, int orderBy = 1);
        Task InitializeTokensFromConfigAsync();
        Task<string> GetAccessTokenAsync();

    }
}
