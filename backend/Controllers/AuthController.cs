using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;
    private readonly PasswordService _passwordService;

    public AuthController(AppDbContext context, JwtService jwtService, PasswordService passwordService)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordService = passwordService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return BadRequest(new { message = "Username already exists" });
        }

        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Email = request.Email
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var (token, expiresAt) = _jwtService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Username, expiresAt));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        
        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        var (token, expiresAt) = _jwtService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Username, expiresAt));
    }
}
