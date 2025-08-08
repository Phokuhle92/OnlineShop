using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.API.Models;
using OnlineShop.API.Models.DTOs;
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using System.Collections.Generic;

namespace OnlineShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        private static readonly ConcurrentDictionary<string, OtpEntry> _otpStore = new();

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        private bool IsValidEmail(string email) =>
            !string.IsNullOrEmpty(email) &&
            Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private string GenerateOtp() =>
            new Random().Next(100000, 999999).ToString();

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["EmailSettings:SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["EmailSettings:SmtpHost"],
                int.Parse(_config["EmailSettings:SmtpPort"]),
                MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(
                _config["EmailSettings:SenderEmail"],
                _config["EmailSettings:SenderPassword"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            if (request == null || !IsValidEmail(request.Email))
                return BadRequest(new { message = "Valid email is required." });

            var otpCode = GenerateOtp();

            var otpEntry = new OtpEntry
            {
                Email = request.Email,
                OtpCode = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false
            };

            _otpStore.AddOrUpdate(request.Email, otpEntry, (key, old) => otpEntry);

            await SendEmailAsync(request.Email, "Your OTP Code", $"Your OTP code is {otpCode}");

            return Ok(new { message = "OTP sent to email" });
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOnlyOtpDto otpRequest)
        {
            if (!_otpStore.TryGetValue(otpRequest.Email, out var storedOtp))
                return BadRequest(new { message = "OTP not found. Please request a new OTP." });

            if (storedOtp.ExpiryTime < DateTime.UtcNow)
            {
                _otpStore.TryRemove(otpRequest.Email, out _);
                return BadRequest(new { message = "OTP expired. Please request a new OTP." });
            }

            if (storedOtp.OtpCode != otpRequest.OtpCode)
                return BadRequest(new { message = "Invalid OTP." });

            storedOtp.IsVerified = true;
            _otpStore[otpRequest.Email] = storedOtp;

            return Ok(new { message = "OTP verified successfully" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!_otpStore.TryGetValue(dto.Email, out var otpEntry) || !otpEntry.IsVerified)
                return BadRequest(new { message = "Email not verified via OTP." });

            var roleName = string.IsNullOrWhiteSpace(dto.Role) ? "Customer" : dto.Role;

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                    return BadRequest(new { message = "Failed to create role." });
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                Surname = dto.Surname
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, roleName);

            // Remove OTP after successful registration
            _otpStore.TryRemove(dto.Email, out _);

            return Ok(new { message = "User registered successfully. You can now login." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials");

            // **No email confirmation check here**

            // Generate and send OTP after successful login credentials verification
            var otpCode = GenerateOtp();
            var otpEntry = new OtpEntry
            {
                Email = dto.Email,
                OtpCode = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false
            };
            _otpStore.AddOrUpdate(dto.Email, otpEntry, (key, old) => otpEntry);

            await SendEmailAsync(dto.Email, "Your Login OTP Code", $"Your OTP code is {otpCode}");

            var roles = await _userManager.GetRolesAsync(user);
            string dashboard = GetDashboardByRoles(roles);

            // Do NOT return token yet, require OTP verification first
            return Ok(new
            {
                message = "Login successful. OTP sent to your email. Please verify OTP to complete login.",
                user = new { user.Id, user.Email, user.UserName, Roles = roles },
                dashboard
            });
        }

        [HttpPost("verify-otp-login")]
        public async Task<IActionResult> VerifyOtpLogin([FromBody] VerifyOnlyOtpDto otpRequest)
        {
            if (!_otpStore.TryGetValue(otpRequest.Email, out var storedOtp))
                return BadRequest(new { message = "OTP not found. Please request a new OTP." });

            if (storedOtp.ExpiryTime < DateTime.UtcNow)
            {
                _otpStore.TryRemove(otpRequest.Email, out _);
                return BadRequest(new { message = "OTP expired. Please request a new OTP." });
            }

            if (storedOtp.OtpCode != otpRequest.OtpCode)
                return BadRequest(new { message = "Invalid OTP." });

            storedOtp.IsVerified = true;
            _otpStore.TryRemove(otpRequest.Email, out _); // Remove OTP after successful verification

            var user = await _userManager.FindByEmailAsync(otpRequest.Email);
            if (user == null)
                return BadRequest(new { message = "User not found." });

            var roles = await _userManager.GetRolesAsync(user);
            string dashboard = GetDashboardByRoles(roles);

            // Return user info without JWT token
            return Ok(new
            {
                message = "OTP verified, login complete.",
                user = new { user.Id, user.Email, user.UserName, Roles = roles },
                dashboard
            });
        }

        private string GetDashboardByRoles(IList<string> roles)
        {
            if (roles.Contains("Admin")) return "/admin/dashboard";
            if (roles.Contains("ProductOwner")) return "/productowner/dashboard";
            if (roles.Contains("StoreUser")) return "/storeuser/dashboard";
            if (roles.Contains("Manager")) return "/manager/dashboard";
            if (roles.Contains("Customer")) return "/customer/dashboard";
            return "/dashboard";
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!IsValidEmail(dto.Email))
                return BadRequest(new { message = "Valid email is required." });

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new { message = "User not found." });

            var otpCode = GenerateOtp();

            var otpEntry = new OtpEntry
            {
                Email = dto.Email,
                OtpCode = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false
            };

            _otpStore.AddOrUpdate(dto.Email, otpEntry, (key, old) => otpEntry);

            await SendEmailAsync(dto.Email, "Password Reset OTP", $"Your OTP code to reset your password is: {otpCode}");

            return Ok(new { message = "OTP sent to your email. Use it to reset your password." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordWithOtp([FromBody] ResetPasswordWithOtpDto dto)
        {
            if (!IsValidEmail(dto.Email))
                return BadRequest(new { message = "Valid email is required." });

            if (!_otpStore.TryGetValue(dto.Email, out var storedOtp))
                return BadRequest(new { message = "OTP not found. Please request a new OTP." });

            if (storedOtp.ExpiryTime < DateTime.UtcNow)
            {
                _otpStore.TryRemove(dto.Email, out _);
                return BadRequest(new { message = "OTP expired. Please request a new OTP." });
            }

            if (storedOtp.OtpCode != dto.OtpCode)
                return BadRequest(new { message = "Invalid OTP." });

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new { message = "User not found." });

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);
            if (!resetResult.Succeeded)
                return BadRequest(resetResult.Errors);

            _otpStore.TryRemove(dto.Email, out _);

            return Ok(new { message = "Password has been reset successfully." });
        }

    }
}
