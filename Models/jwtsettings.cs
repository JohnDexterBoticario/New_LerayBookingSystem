namespace New_LeRayBookingSystem.Models
{
    // Class to map configuration settings from appsettings.json
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationHours { get; set; } = 2; // Token validity period
    }
}
