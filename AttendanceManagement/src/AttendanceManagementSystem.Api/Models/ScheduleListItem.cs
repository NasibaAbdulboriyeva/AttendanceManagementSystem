using AttendanceManagementSystem.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Api.Models
{
    public class ScheduleListItem
    {
        public long EmployeeId { get; set; }
        public long EmployeeScheduleId { get; set; }

        [Display(Name = "Ф.И.О.Cотрудника")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Required field")]
        [Display(Name = "Время прихода")]
        public TimeSpan StartTime { get; set; } 

        [Display(Name = "Время уходa")]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Лимит (min)")]
        public int LimitInMinutes { get; set; } 

        [Display(Name = "EmployementType")]
        public EmployementType EmployementType { get; set; }
    }
}
