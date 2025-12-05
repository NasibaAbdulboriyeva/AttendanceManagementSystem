using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Application.DTOs
{
    public class UpdateWorkingDayRequest
    {
        public long EmployeeId { get; set; }
        public DateTime EntryDay { get; set; }
        public bool IsWorkingDay { get; set; }
    }
}
