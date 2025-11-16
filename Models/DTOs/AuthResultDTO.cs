namespace New_LeRayBookingSystem.Models.DTOs
{
    public class AuthResultDto
    {
        // 1. Indicates if the operation (Register/Login/OTP) was successful
        public bool IsSuccess { get; set; }

        // 2. Contains the specific error message or a success message
        public string Message { get; set; } = string.Empty;

        // --- NEW FIELDS FOR AUTHENTICATION FLOW MANAGEMENT ---

        // 3. Flag to tell the client: "Do NOT proceed to the main app, an OTP/email confirmation is required."
        public bool RequiresEmailVerification { get; set; }

        // 4. Flag to tell the client: "Do NOT proceed to the main app, the user needs to enter an MFA code."
        public bool RequiresMfa { get; set; }

        // 5. Contains the JWT token, User ID, Roles, etc., on FINAL success (when IsSuccess is true 
        //    AND both RequiresEmailVerification and RequiresMfa are false).
        //    It can also temporarily hold required data for the next step (e.g., User ID for OTP step).
        public object? Data { get; set; }
    }
}