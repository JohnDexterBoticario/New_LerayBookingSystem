
namespace New_LeRayBookingSystem.Models// <-- Correct namespace
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user);
    }
}