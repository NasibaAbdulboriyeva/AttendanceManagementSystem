
namespace AttendanceManagementSystem.Application.DTOs
{
    public class TTLockResponse
    {
        public List<TTLockRecordDto> List { get; set; } = new List<TTLockRecordDto>();
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int Pages { get; set; }
        public int Total { get; set; }
    }
}
