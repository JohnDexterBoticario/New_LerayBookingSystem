namespace New_LeRayBookingSystem.Models.DTOs
{
    public class AuthResultDto
    {
        // Indicates if the operation (Register/Login/OTP) was successful
        public bool IsSuccess { get; set; }

        // Contains the specific error message (e.g., "Password too short") or a success message
        public string Message { get; set; } = string.Empty;

        // Contains the JWT token, User ID, Roles, etc., on success
        public object? Data { get; set; } 
    }
}