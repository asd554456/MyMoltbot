namespace Backend.DTOs;

public record RegisterRequest(string Username, string Password, string? Email);
public record LoginRequest(string Username, string Password);
public record AuthResponse(string Token, string Username, DateTime ExpiresAt);
