using System;
using System.Collections.Generic;
using System.Linq;

namespace AttendanceManagementSystem.Application.DTOs
{
    public class CalculatedDailyLogDto
    {
        public DateTime Date { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeFullName { get; set; }
        public DateTime? FirstCheckInTime { get; set; }//null boladi agar hodim kemagan bosa 
        public TimeSpan ScheduledStartTime { get; set; }
        public int LateMinutesTotal { get; set; }
        public int LateMinutesBeyondLimit { get; set; }

    }
}
