namespace Sadalene.API.DTOs.Auth;

public record AuthResponse(string Token, string IdentityType, int Id, string DisplayName, string Phone);
