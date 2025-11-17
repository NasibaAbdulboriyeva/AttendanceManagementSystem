using System;

namespace AttendanceManagementSystem.Application.Services
{
    public class XlsxMappingHelper
    {
        public static string ExtractICNumber(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return string.Empty;
            }

            var parts = username.Split(new char[] { '-', ' ' }, 2);

            return parts.Length > 0 && parts[0].All(char.IsDigit) ? parts[0] : null;
        }
    }
}
