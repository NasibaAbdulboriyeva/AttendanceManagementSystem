using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Application.DTOs
{
    public class EmployeeSummaryDto
    {
        public string EmployeeCode { get; set; }
        public string EmployeeFullName { get; set; } 

        public int Year { get; set; }
        public int Month { get; set; }

        public int TotalDaysPresent { get; set; }
        public int TotalLateArrivalMinutes { get; set; } 
        public int TotalLateMinutesBeyondLimit { get; set; } 

        public decimal DisciplinaryPenaltyPercentage { get; set; }
        public int RemainingLateLimitMinutes { get; set; }

        public DateTime CalculatedAt { get; set; }
    }
}
