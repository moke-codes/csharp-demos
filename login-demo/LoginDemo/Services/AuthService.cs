using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoginDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using Microsoft.Extensions.Logging;

namespace LoginDemo.Services;

public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IEmailSender<ApplicationUser> emailSender,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
    {
        _logger.LogInformation("Attempting to reset password for user {Email}. Token length: {TokenLength}", 
            user.Email, token.Length);
        _logger.LogInformation("Token: {Token}", token);
        
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        
        if (!result.Succeeded)
        {
            _logger.LogWarning("Password reset failed for user {Email}. Errors: {Errors}", 
                user.Email, 
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        else
        {
            _logger.LogInformation("Password reset successful for user: {Email}", user.Email);
        }
        
        return result;
    }

    public async Task<(bool success, string token)> LoginAsync(string email, string password, bool rememberMe)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return (false, string.Empty);

        var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, false);
        if (!result.Succeeded)
            return (false, string.Empty);

        var token = GenerateJwtToken(user, rememberMe);
        return (true, token);
    }

    public async Task<(bool success, string message)> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

        return (true, "User registered successfully");
    }

    public async Task<(bool success, string message)> EnableTwoFactorAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, "User not found");

        var secret = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(secret))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            secret = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        user.TwoFactorEnabled = true;
        user.TwoFactorSecret = secret;
        await _userManager.UpdateAsync(user);

        return (true, secret ?? "Failed to generate secret");
    }

    public async Task<(bool success, string message)> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
            return (false, "If your email is registered, you will receive a password reset link.");
        }

        // Generate a new security stamp to invalidate any existing tokens
        await _userManager.UpdateSecurityStampAsync(user);
        
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        _logger.LogInformation("Generated reset token for user {Email}. Token length: {TokenLength}", 
            email, token.Length);
        _logger.LogInformation("Token: {Token}", token);

        var resetLink = $"{_configuration["AppUrl"]}/reset-password?token={WebUtility.UrlEncode(token)}&email={WebUtility.UrlEncode(email)}";
        _logger.LogInformation("Reset link generated: {ResetLink}", resetLink);
        
        await _emailSender.SendPasswordResetLinkAsync(user, email, resetLink);
        return (true, "If your email is registered, you will receive a password reset link.");
    }

    private string GenerateJwtToken(ApplicationUser user, bool rememberMe = false)
    {
        if (user.Email == null || user.UserName == null)
            throw new InvalidOperationException("User email and username cannot be null");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("firstName", user.FirstName ?? string.Empty),
            new Claim("lastName", user.LastName ?? string.Empty)
        };

        var jwtSecret = _configuration["JWT:Secret"];
        if (string.IsNullOrEmpty(jwtSecret))
            throw new InvalidOperationException("JWT Secret is not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        // Set token expiration based on remember me option
        var expires = rememberMe 
            ? DateTime.Now.AddDays(30) // 30 days for remember me
            : DateTime.Now.AddDays(Convert.ToDouble(_configuration["JWT:ExpireDays"])); // Default expiration

        var token = new JwtSecurityToken(
            _configuration["JWT:ValidIssuer"],
            _configuration["JWT:ValidAudience"],
            claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
} 