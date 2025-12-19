namespace AttendanceManagementSystem.Domain.Entities
{
    public class Employee
    {
        public long EmployeeId { get; set; }
        public string UserName { get; set; }
        public bool IsActive { get; set; }
        public int? CardId { get; set; }//returned from TTLock when it is created 
        public int? FingerprintId { get; set; }//returned from TTLock when it is created 
        public string? FingerprintNumber{ get; set; }
        public string? CardNumber { get; set; }//ic or finger card number
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public ICollection<AttendanceLog> AttendanceLogs { get; set; }
        public ICollection<CurrentAttendanceLog> CurrentAttendanceLogs { get; set; }
        public ICollection<EmployeeScheduleHistory> EmployeeScheduleHistories { get; set; }
        public EmployeeSchedule EmployeeSchedule { get; set; }
    }
}
