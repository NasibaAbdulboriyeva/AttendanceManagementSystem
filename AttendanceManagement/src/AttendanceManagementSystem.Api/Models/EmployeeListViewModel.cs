
namespace AttendanceManagementSystem.Api.Models
{
    public class EmployeeListViewModel
    {
       public ICollection<EmployeeListItem> Employees { get; set; } = new List<EmployeeListItem>();
    }
}
