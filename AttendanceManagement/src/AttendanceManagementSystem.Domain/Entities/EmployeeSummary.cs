namespace AttendanceManagementSystem.Domain.Entities
{
    public class EmployeeSummary
    {
        public long EmployeeSummaryId { get; set; }
        public long EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public Month Month { get; set; }
        public int LateArrivalMinutes { get; set; }//obshiysi
        public int LateMinutesBeyondLimit { get; set; }//80 minut ayrilgani
        public int DisciplinaryPenalty { get; set; }//% 
        public DateTime  CalculatedAt { get; set; }
        public DateTime  CreatedAt { get; set; }
    }
}
