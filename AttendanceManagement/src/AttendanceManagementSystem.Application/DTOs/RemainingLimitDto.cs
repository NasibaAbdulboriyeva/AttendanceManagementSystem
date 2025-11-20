namespace AttendanceManagementSystem.Application.DTOs
{
    public class RemainingLimitDto
    {
        public int RemainingMinutes { get; set; }
        public int TotalMinutesUsed { get; set; }
        public bool IsLimitExceeded { get; set; }
    }
}
