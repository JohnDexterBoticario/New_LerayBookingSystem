using New_LeRayBookingSystem.Models;
using New_LeRayBookingSystem.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace New_LeRayBookingSystem.Services
{
    public class AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        // You MUST create this interface/service for JWT generation
        private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

        // --- REGISTER IMPLEMENTATION ---
        public async Task<AuthResultDto> RegisterAsync(RegisterDto registerDto)
        {
            // 1. Check if user already exists
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
            {
                return new AuthResultDto { IsSuccess = false, Message = "User with this email already exists." };
            }

            // 2. Create ApplicationUser model (assuming your ApplicationUser constructor or properties handle all DTO fields)
            var user = new ApplicationUser 
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                // Map other necessary fields from DTO to ApplicationUser
                // e.g., FirstName = registerDto.FirstName, 
                // e.g., PhoneNumber = registerDto.Phone, 
            };
            
            // 3. Create user with password
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                // 4. Assign default role
                if (await _roleManager.RoleExistsAsync("Client"))
                {
                    await _userManager.AddToRoleAsync(user, "Client");
                }
                
                // You should implement OTP sending logic here

                return new AuthResultDto { IsSuccess = true, Message = "Registration successful. Please proceed to verification." };
            }
            else
            {
                // 5. CAPTURE AND RETURN DETAILED IDENTITY ERRORS
                var errors = result.Errors.Select(e => e.Description);
                var errorMessage = string.Join(". ", errors);
                return new AuthResultDto { IsSuccess = false, Message = errorMessage };
            }
        }

        // --- LOGIN IMPLEMENTATION ---
        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
           var user = await _userManager.FindByEmailAsync(loginDto.EmailOrUsername);

            if (user == null)
            {
                return new AuthResultDto { IsSuccess = false, Message = "Invalid email or password." };
            }

            // Use SigninManager to check password and handle lockout features
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (result.Succeeded)
            {
                // 1. Generate JWT Token
                var token = _jwtTokenGenerator.GenerateToken(user);
                
                // 2. SUCCESS: Return Token and User Info
                return new AuthResultDto
                {
                    IsSuccess = true,
                    Message = "Login successful.",
                    Data = new 
                    {
                        Token = token,
                        UserId = user.Id,
                        Name = user.Email, // Replace with user.FirstName/FullName if available
                        Roles = await _userManager.GetRolesAsync(user)
                    }
                };
            }
            else if (result.IsLockedOut)
            {
                 return new AuthResultDto { IsSuccess = false, Message = "Account is locked out." };
            }
            // Add else if (result.RequiresTwoFactor) { ... } if you implement MFA
            else
            {
                return new AuthResultDto { IsSuccess = false, Message = "Invalid email or password." };
            }
        }
        
        // --- VERIFICATION / MFA IMPLEMENTATIONS (PLACEHOLDER) ---
        public Task<AuthResultDto> VerifyOtpAsync(OtpVerifyDto verifyDto)
        {
            // Placeholder: Implement OTP verification logic
            return Task.FromResult(new AuthResultDto { IsSuccess = false, Message = "OTP verification not implemented." });
        }

        public Task<AuthResultDto> VerifyMfaAsync(OtpVerifyDto verifyDto)
        {
            // Placeholder: Implement MFA verification logic
            return Task.FromResult(new AuthResultDto { IsSuccess = false, Message = "MFA verification not implemented." });
        }
    }
}