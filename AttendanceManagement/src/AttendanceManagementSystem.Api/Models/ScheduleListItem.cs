using AttendanceManagementSystem.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace AttendanceManagementSystem.Api.Models
{
    public class ScheduleListItem
    {
        public long EmployeeId { get; set; }
        public long EmployeeScheduleId { get; set; }

        [Display(Name = "EmployeeName")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vaqt talab qilinadi")]
        [Display(Name = "Boshlanishi")]
        public TimeSpan StartTime { get; set; } 

        [Required(ErrorMessage = "Vaqt talab qilinadi")]
        [Display(Name = "Tugashi")]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "Limit (min)")]
        public int LimitInMinutes { get; set; } 

        [Display(Name = "Turi")]
        public EmployementType EmployementType { get; set; }
    }
}
