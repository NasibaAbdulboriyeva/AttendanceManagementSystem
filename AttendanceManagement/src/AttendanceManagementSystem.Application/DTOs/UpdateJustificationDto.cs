using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Application.DTOs
{
   
    public class UpdateJustificationDto
    {
        public long EmployeeId { get; set; }
        public DateOnly EntryDay { get; set; }
        public bool IsJustified { get; set; }
    }
}
