using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Application.DTOs
{
    public class UpdateEntryTimeDto
    {
        public long EmployeeId { get; set; }
        public DateOnly EntryDay { get; set; }
        public TimeSpan ManualEntryTime { get; set; }
        public string Description { get; set; }
    }
}
