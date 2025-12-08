using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Application.DTOs
{
    public class DescriptionUpdateDto
    {
        public int EmployeeId { get; set; }
        public DateOnly EntryDay { get; set; }
        public string? Description  { get; set; }
    }
}
