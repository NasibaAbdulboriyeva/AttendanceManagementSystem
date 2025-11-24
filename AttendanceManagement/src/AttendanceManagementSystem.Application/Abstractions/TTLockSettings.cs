namespace AttendanceManagementSystem.Api.Configurations
{
    public class TTLockSettings
    {
        public string BaseUrl { get; set; }
        public string LockId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
