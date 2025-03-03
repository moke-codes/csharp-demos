using LoginDemo.Models;
using LoginDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace LoginDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, token) = await _authService.LoginAsync(request.Email, request.Password, request.RememberMe);
        
        if (!success)
            return BadRequest(new { message = "Invalid email or password" });

        return Ok(new { token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, message) = await _authService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    [HttpPost("enable-2fa")]
    public async Task<IActionResult> EnableTwoFactor([FromBody] EnableTwoFactorRequest request)
    {
        var (success, message) = await _authService.EnableTwoFactorAsync(request.UserId);
        
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var (success, message) = await _authService.ForgotPasswordAsync(request.Email);
        
        if (!success)
            return BadRequest(new { message });

        return Ok(new { message });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            _logger.LogWarning("Reset password attempt with null or empty token");
            return BadRequest(new { message = "Invalid reset link. Please request a new password reset." });
        }

        _logger.LogInformation("Reset password attempt for email: {Email}", request.Email);
        _logger.LogInformation("Token length: {TokenLength}", request.Token.Length);

        var user = await _authService.GetUserByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("User not found for email: {Email}", request.Email);
            return BadRequest(new { message = "Invalid or expired reset link" });
        }

        var result = await _authService.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Password reset failed for user {Email}. Errors: {Errors}", 
                request.Email, 
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        _logger.LogInformation("Password reset successful for user: {Email}", request.Email);
        return Ok(new { message = "Password reset successfully" });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class EnableTwoFactorRequest
{
    public string UserId { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Reset token is required")]
    public string Token { get; set; } = string.Empty;
    [Required(ErrorMessage = "New password is required")]
    public string NewPassword { get; set; } = string.Empty;
} 