namespace Api.Models.Dtos;

public record RegisterRequest(string Email, string Password, string? DisplayName);
public record LoginRequest(string Email, string Password);
public record AuthUserResponse(string Id, string Email, string? DisplayName);
