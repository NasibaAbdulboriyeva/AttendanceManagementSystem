namespace AttendanceManagementSystem.Domain.Entities
{
    public class DoorActivityLog
    {
        public long DoorActivityLogId { get; set; }
        public DateTime RecordedTime { get; set; }
        public EventType Type { get; set; }
        public string DeviceName { get; set; }
        public  DateTime CreatedAt { get; set; }
    }
}