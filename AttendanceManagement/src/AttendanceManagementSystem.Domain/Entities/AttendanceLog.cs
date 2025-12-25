namespace AttendanceManagementSystem.Domain.Entities
{
    public class AttendanceLog
    {
        public long AttendenceLogId { get; set; }
        public long RecordId { get; set; }
        public long EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public DateTime RecordedTime { get; set; }
        public EntryType EntryType { get; set; }
        public AttendanceStatus Status { get; set; }
        public string RawUsername { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
