namespace New_LeRayBookingSystem.Models.DTOs
{
    public class UserDto
    {
       public required string Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
    }
}