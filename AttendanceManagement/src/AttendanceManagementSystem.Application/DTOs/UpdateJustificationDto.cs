

namespace AttendanceManagementSystem.Application.DTOs
{
   
    public class UpdateJustificationDto
    {
        public long EmployeeId { get; set; }
        public DateOnly EntryDay { get; set; }
        public bool IsJustified { get; set; }
        public bool IsWorkingDay { get; set; }
    }
}
