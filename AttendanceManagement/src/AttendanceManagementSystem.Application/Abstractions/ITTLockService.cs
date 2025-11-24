using AttendanceManagementSystem.Application.DTOs;

namespace AttendanceManagementSystem.Application.Abstractions
{
    public interface ITTLockService
    {
        // 1. Attendance Loglarini olish (Barcha sahifalar, minimal parametrlar)
        Task<ICollection<TTLockRecordDto>> GetAllAttendanceLockRecordsAsync(
       
            long startDate,
            long endDate,
            int? recordType = null);

        // 2. IC Card ro'yxatini olish (Barcha sahifalar)
        Task<ICollection<TTLockIcCardDto>> GetAllIcCardRecordsAsync(
         
            string? searchStr = null,
            int orderBy = 1);

        // 3. Barmoq Izi ro'yxatini olish (Barcha sahifalar, DTO qaytarish)
        Task<ICollection<TTLockFingerprintDto>> GetAllFingerprintsPaginatedAsync(
      
            string? searchStr = null,
            int orderBy = 1);

    }
}
