using New_LeRayBookingSystem.Models;
using New_LeRayBookingSystem.Models.DTOs;
using Microsoft.AspNetCore.Identity;

namespace New_LeRayBookingSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
        }

        // =======================
        // REGISTER + SEND OTP
        // =======================
        public async Task<AuthResultDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
                return new AuthResultDto { IsSuccess = false, Message = "User with this email already exists." };

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
               FullName = registerDto.FirstName + " " + (registerDto.LastName ?? "User")
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                string errors = string.Join(". ", result.Errors.Select(e => e.Description));
                return new AuthResultDto { IsSuccess = false, Message = errors };
            }

            // Assign default role
            if (await _roleManager.RoleExistsAsync("Client"))
                await _userManager.AddToRoleAsync(user, "Client");

            // Send OTP immediately
            await SendOtpAsync(user.Email);

            return new AuthResultDto
            {
                IsSuccess = true,
                Message = "Registration successful. OTP has been sent to your email."
            };
        }

        // =======================
        // LOGIN
        // =======================
        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.EmailOrUsername)
                     ?? await _userManager.FindByNameAsync(loginDto.EmailOrUsername);

            if (user == null)
                return new AuthResultDto { IsSuccess = false, Message = "Invalid email or password." };

            var check = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!check.Succeeded)
                return new AuthResultDto { IsSuccess = false, Message = "Invalid email or password." };

            // Optionally check if OTP is verified
            if (!string.IsNullOrEmpty(user.TwoFactorCode))
                return new AuthResultDto { IsSuccess = false, Message = "Please verify OTP first." };

            string token = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResultDto
            {
                IsSuccess = true,
                Message = "Login successful.",
                Data = new
                {
                    Token = token,
                    UserId = user.Id,
                    Name = user.FullName,
                    Roles = await _userManager.GetRolesAsync(user)
                }
            };
        }

        // =======================
        // SEND OTP
        // =======================
        public async Task<AuthResultDto> SendOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new AuthResultDto { IsSuccess = false, Message = "User not found." };

            string otp = GenerateOtp();
            user.TwoFactorCode = otp;
            user.TwoFactorCodeExpiry = DateTime.UtcNow.AddMinutes(5);
            await _userManager.UpdateAsync(user);

            string subject = "Your Verification Code";
            string html = $@"
                <h2>Hello {user.FullName},</h2>
                <p>Your OTP Code is:</p>
                <h1 style='letter-spacing:4px;'>{otp}</h1>
                <p>This code will expire in <b>5 minutes</b>.</p>
            ";

            await _emailService.SendEmailAsync(user.Email, subject, html);
            return new AuthResultDto { IsSuccess = true, Message = "OTP has been sent." };
        }

        // =======================
        // VERIFY OTP + RETURN JWT
        // =======================
        public async Task<AuthResultDto> VerifyOtpAsync(OtpVerifyDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return new AuthResultDto { IsSuccess = false, Message = "User not found." };

            if (user.TwoFactorCode != dto.Code)
                return new AuthResultDto { IsSuccess = false, Message = "Invalid OTP." };

            if (user.TwoFactorCodeExpiry < DateTime.UtcNow)
                return new AuthResultDto { IsSuccess = false, Message = "OTP expired." };

            // Clear OTP after success
            user.TwoFactorCode = null;
            user.TwoFactorCodeExpiry = null;
            await _userManager.UpdateAsync(user);

            string token = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResultDto
            {
                IsSuccess = true,
                Message = "OTP verified successfully.",
                Data = new
                {
                    Token = token,
                    UserId = user.Id,
                    Name = user.FullName,
                    Roles = await _userManager.GetRolesAsync(user)
                }
            };
        }

        // =======================
        // VERIFY MFA (Optional)
        // =======================
        public async Task<AuthResultDto> VerifyMfaAsync(OtpVerifyDto dto)
        {
            // For simplicity, reuse VerifyOtpAsync
            return await VerifyOtpAsync(dto);
        }

        // =======================
        // OTP GENERATOR
        // =======================
        private string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }
    }
}
