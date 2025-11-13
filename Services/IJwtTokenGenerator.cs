using New_LeRayBookingSystem.Models;
// You may need to add Identity, or Jwt related usings if the full signature is complex
// but for the interface, the model is usually sufficient.

namespace New_LeRayBookingSystem.Services
{
    // Interface for generating JWT tokens
    public interface IJwtTokenGenerator
    {
        // Generates a JWT token based on the user's details and claims.
        string GenerateToken(ApplicationUser user);
    }
}