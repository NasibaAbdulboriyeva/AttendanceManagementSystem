using AttendanceManagementSystem.Application.DTOs;

namespace AttendanceManagementSystem.Application.Services
{
    public interface IUploadService
    {
        Task<int> UploadAttendanceLogAsync(UploadRequestDto file);
        Task<int> UploadScheduleAsync(UploadRequestDto file);
    }
}
