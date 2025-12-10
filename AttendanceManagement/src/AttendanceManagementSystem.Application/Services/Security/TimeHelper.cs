
namespace AttendanceManagementSystem.Application.Services.Security
{
    public class TimeHelper
    {
        public static DateTime GetDateTime()
        {
            return DateTime.UtcNow.AddHours(5);
        }
    }
}
