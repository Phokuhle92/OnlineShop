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
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        private static readonly ConcurrentDictionary<string, OtpEntry> _otpStore = new();

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
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

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);

            _otpStore.TryRemove(dto.Email, out _);

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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!IsValidEmail(dto.Email))
                return BadRequest("Valid email is required.");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest("User not found.");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            string resetLink = $"{_config["FrontendUrl"]}/reset-password?email={Uri.EscapeDataString(dto.Email)}&token={Uri.EscapeDataString(resetToken)}";

            await SendEmailAsync(dto.Email, "Password Reset Request",
                $"You requested a password reset. Use the following link:\n\n{resetLink}\n\nIf you did not request this, ignore this email.");

            return Ok("Password reset email sent.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!IsValidEmail(dto.Email))
                return BadRequest("Valid email is required.");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest("User not found.");

            var decodedToken = Uri.UnescapeDataString(dto.Token);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password has been reset successfully.");
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = _config["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("JWT Secret Key is missing in configuration");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "OnlineShop",
                audience: _config["Jwt:Audience"] ?? "OnlineShopUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
