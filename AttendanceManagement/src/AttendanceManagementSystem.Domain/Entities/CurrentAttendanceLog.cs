namespace AttendanceManagementSystem.Domain.Entities
{
    public class CurrentAttendanceLog
    {
        public long CurrentAttendanceLogId { get; set; }
        public long EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int LateArrivalMinutes { get; set; }// kunlik 
        public int RemainingLateMinutes { get; set; }// 80 minut ayrilgani
        public string? Description { get; set; }
        public bool IsJustified { get; set; }// ogohlatrlganmi
        public DateTime  CalculatedAt { get; set; }
        public TimeOnly  FirstEntryTime { get; set; }
        public DateOnly EntryDay { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
