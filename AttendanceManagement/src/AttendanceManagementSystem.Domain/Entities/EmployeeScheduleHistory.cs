using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Domain.Entities
{
    public class EmployeeScheduleHistory
    {
        public long EmployeeScheduleHistoryId { get; set; }
        public long EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int LimitInMinutes { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public EmployementType EmployementType { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
}
