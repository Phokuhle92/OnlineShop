using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OnlineShop.API.Models;
using OnlineShop.API.Models.DTOs;
using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace OnlineShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        // Thread-safe in-memory OTP store
        private static readonly ConcurrentDictionary<string, OtpEntry> _otpStore = new();

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        // Validate email format
        private bool IsValidEmail(string email) =>
            !string.IsNullOrEmpty(email) &&
            Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        // Generate 6-digit OTP code
        private string GenerateOtp() =>
            new Random().Next(100000, 999999).ToString();

        // Send email using MailKit
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
                return BadRequest("Valid email is required.");

            var otpCode = GenerateOtp();

            var otpEntry = new OtpEntry
            {
                Email = request.Email,
                OtpCode = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false
            };

            _otpStore.AddOrUpdate(request.Email, otpEntry, (key, old) => otpEntry);

            // Send OTP email
            await SendEmailAsync(request.Email, "Your OTP Code", $"Your OTP code is {otpCode}");

            return Ok(new { message = "OTP sent to email" });
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOnlyOtpDto otpRequest)
        {
            if (!_otpStore.TryGetValue(otpRequest.Email, out var storedOtp))
                return BadRequest("OTP not found. Please request a new OTP.");

            if (storedOtp.ExpiryTime < DateTime.UtcNow)
            {
                _otpStore.TryRemove(otpRequest.Email, out _);
                return BadRequest("OTP expired. Please request a new OTP.");
            }

            if (storedOtp.OtpCode != otpRequest.OtpCode)
                return BadRequest("Invalid OTP.");

            storedOtp.IsVerified = true;
            _otpStore[otpRequest.Email] = storedOtp;

            return Ok("OTP verified successfully");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!_otpStore.TryGetValue(dto.Email, out var otpEntry) || !otpEntry.IsVerified)
                return BadRequest("Email not verified via OTP.");

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

            // Automatically confirm email after successful OTP verification and registration
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);

            _otpStore.TryRemove(dto.Email, out _); // Cleanup OTP

            return Ok("User registered and email confirmed.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials");

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized("Email not confirmed");

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                message = "Login successful",
                user = new { user.Id, user.Email, user.UserName }
            });
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var jwtKey = _config["Jwt:Key"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // DTO classes
        public class SendOtpRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        private class OtpEntry
        {
            public string Email { get; set; } = string.Empty;
            public string OtpCode { get; set; } = string.Empty;
            public DateTime ExpiryTime { get; set; }
            public bool IsVerified { get; set; }
        }
    }
}
