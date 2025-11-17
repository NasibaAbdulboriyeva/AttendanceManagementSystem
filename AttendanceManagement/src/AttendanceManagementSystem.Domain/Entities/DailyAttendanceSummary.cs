using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Domain.Entities
{
    public class DailyAttendanceSummary
    {
        public long DailyAttendanceSummaryId { get; set; }
        public long EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public DateTime MyProperty { get; set; }
    }
}
