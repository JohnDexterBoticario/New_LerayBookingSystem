namespace New_LeRayBookingSystem.Models.DTOs
{
    public class CreateServiceDto
    {
       public string ServiceName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsBundle { get; set; }
        public string? Description { get; set; }
    }
}
