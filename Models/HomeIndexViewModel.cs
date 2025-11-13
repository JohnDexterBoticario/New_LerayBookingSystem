namespace New_LeRayBookingSystem.Models
{
    public class ServiceOption
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ComboBundle
    {
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public List<string> Inclusions { get; set; } = new();
    }

    public class Testimonial
    {
        public string Quote { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int Rating { get; set; } = 5;
    }

    public class HomeIndexViewModel
    {
        public List<ServiceOption> Services { get; set; } = new();
        public List<ComboBundle> Bundles { get; set; } = new();
        public List<Testimonial> Testimonials { get; set; } = new();
    }
}
