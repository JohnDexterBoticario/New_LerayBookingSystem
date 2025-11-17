namespace New_LeRayBookingSystem.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public bool UseSSL { get; set; } = true;
        public string SenderName { get; set; } = "LeRayAestheticsCenter";
        public string SenderEmail { get; set; } = "boticatiojohndexter@gmail.com";
        public string Username { get; set; } = "boticatiojohndexter@gmail.com";
        public string Password { get; set; } = "wbzwxmppwoicxjjv";
    }
}

