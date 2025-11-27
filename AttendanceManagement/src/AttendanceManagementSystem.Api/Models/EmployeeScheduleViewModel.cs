using AttendanceManagementSystem.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Api.Models
{
        public class EmployeeScheduleViewModel
        {
        public List<ScheduleListItem> Schedules { get; set; } = new List<ScheduleListItem>();

        public string? Message { get; set; }
    }

}
