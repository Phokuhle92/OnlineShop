using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OnlineShop.API.Helpers;
using OnlineShop.API.Models;
using OnlineShop.API.Models.DTOs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OnlineShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        // Store OTPs temporarily
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

        #region Helpers
        private bool IsValidEmail(string email) =>
            !string.IsNullOrEmpty(email) &&
            Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private string GenerateOtp() =>
            new Random().Next(100000, 999999).ToString();

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var senderPassword = _config["EmailSettings:SenderPassword"];
            var smtpHost = _config["EmailSettings:SmtpHost"];
            int smtpPort = int.TryParse(_config["EmailSettings:SmtpPort"], out var port) ? port : 587;

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(senderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart("plain") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(senderEmail, senderPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        private IActionResult ValidateOtp(string key, string otpCode)
        {
            if (!_otpStore.TryGetValue(key, out var storedOtp))
                return BadRequest(new { message = "OTP not found. Please request a new OTP." });

            if (storedOtp.ExpiryTime < DateTime.UtcNow)
            {
                _otpStore.TryRemove(key, out _);
                return BadRequest(new { message = "OTP expired. Please request a new OTP." });
            }

            if (storedOtp.OtpCode != otpCode)
                return BadRequest(new { message = "Invalid OTP." });

            storedOtp.IsVerified = true;
            _otpStore[key] = storedOtp;

            return Ok(new { message = "OTP verified successfully." });
        }

        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var keyString = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is not configured in appsettings.json.");
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(keyString));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion // Helpers

        #region Registration
        [HttpPost("send-registration-otp")]
        public async Task<IActionResult> SendRegistrationOtp([FromBody] SendOtpRequestDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email))
                return BadRequest(new { message = "Email is required." });

            if (!IsValidEmail(dto.Email))
                return BadRequest(new { message = "Invalid email." });

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "User already exists." });

            var otpCode = GenerateOtp();
            var key = $"{dto.Email}-Registration"; // no Role now
            _otpStore[key] = new OtpEntry
            {
                Email = dto.Email,
                OtpCode = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false
            };

            await SendEmailAsync(dto.Email, "Your Registration OTP", $"Your OTP code is {otpCode}");
            return Ok(new { message = "OTP sent to your email." });
        }

        [HttpPost("verify-registration-otp")]
        public IActionResult VerifyRegistrationOtp([FromBody] VerifyOnlyOtpDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Invalid payload." });
            var key = $"{dto.Email}-Registration"; // no Role now
            return ValidateOtp(key, dto.OtpCode);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password) || string.IsNullOrEmpty(dto.Role))
                return BadRequest(new { message = "Email, password, and role are required." });

            // Use OTP key without role
            var key = $"{dto.Email}-Registration";
            if (!_otpStore.TryGetValue(key, out var storedOtp) || !storedOtp.IsVerified)
                return BadRequest(new { message = "OTP not verified. Please verify registration OTP first." });

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "User already exists." });

            var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(new { message = "Registration failed.", details = result.Errors });

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                return BadRequest(new { message = $"Role '{dto.Role}' does not exist." });

            await _userManager.AddToRoleAsync(user, dto.Role);

            // Remove OTP after successful registration
            _otpStore.TryRemove(key, out _);

            return Ok(new { message = "User registered successfully." });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password) || string.IsNullOrEmpty(dto.Role))
                return BadRequest(new { message = "Email, password, and role are required." });

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized(new { message = "Invalid credentials." });

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(dto.Role))
                return BadRequest(new { message = $"User does not have the '{dto.Role}' role." });

            var otpCode = GenerateOtp();
            var key = $"{dto.Email}-Login-{dto.Role}";
            _otpStore[key] = new OtpEntry
            {
                Email = dto.Email,
                Role = dto.Role,
                OtpCode = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false
            };

            await SendEmailAsync(dto.Email, "Your Login OTP Code", $"Your OTP code is {otpCode}");
            return Ok(new { message = $"OTP sent to {dto.Role} login email." });
        }

        [HttpPost("verify-login-otp")]
        public async Task<IActionResult> VerifyLoginOtp([FromBody] VerifyLoginOtpDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Invalid payload." });

            var key = $"{dto.Email}-Login-{dto.Role}";
            var validation = ValidateOtp(key, dto.OtpCode);
            if (validation is BadRequestObjectResult) return validation;

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest(new { message = "User not found." });

            _otpStore.TryRemove(key, out _);
            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            string dashboard = dto.Role switch
            {
                "Admin" => "/admin/dashboard",
                "ProductOwner" => "/productowner/dashboard",
                "Customer" => $"/landing/{user.Id}",
                _ => "/"
            };

            return Ok(new { message = "Login successful", data = new { dashboard, token } });
        }
        #endregion // Registration

        #region Forgot Password
        [HttpPost("forgot-password-send-otp")]
        public async Task<IActionResult> ForgotPasswordSendOtp([FromBody] ForgotPasswordDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || !IsValidEmail(dto.Email))
                return BadRequest(new { message = "Valid email is required." });

            var user = await _userManager.FindByEmailAsync(dto.Email!);
            if (user == null)
                return BadRequest(new { message = "User not found." });

            var otpCode = GenerateOtp();
            var key = $"{dto.Email}-ForgotPassword";
            _otpStore[key] = new OtpEntry
            {
                Email = dto.Email,
                Role = "ForgotPassword",
                OtpCode = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false
            };

            await SendEmailAsync(dto.Email, "Forgot Password OTP", $"Your OTP code is {otpCode}");
            return Ok(new { message = "OTP sent to your email." });
        }

        [HttpPost("forgot-password-verify-otp")]
        public IActionResult ForgotPasswordVerifyOtp([FromBody] VerifyOnlyOtpDto dto) =>
            ValidateOtp($"{dto.Email}-ForgotPassword", dto.OtpCode);

        [HttpPost("reset-password-with-otp")]
        public async Task<IActionResult> ResetPasswordWithOtp([FromBody] ResetPasswordWithOtpDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Invalid payload." });

            var key = $"{dto.Email}-ForgotPassword";
            if (!_otpStore.TryGetValue(key, out var storedOtp) || !storedOtp.IsVerified)
                return BadRequest(new { message = "OTP not verified. Please verify OTP first." });

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new { message = "User not found." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Password reset failed.", details = errors });
            }

            _otpStore.TryRemove(key, out _);
            return Ok(new { message = "Password reset successfully. You can now login." });
        }
        #endregion // Forgot Password
    }
}