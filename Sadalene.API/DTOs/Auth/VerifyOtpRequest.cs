namespace Sadalene.API.DTOs.Auth;

public record VerifyOtpRequest(string Phone, string Code);
