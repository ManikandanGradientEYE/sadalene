namespace Sadalene.API.DTOs.Cart;

public record CartDto(
    int Id,
    int ForCustomerId,
    int? DivisionId,
    string? DivisionName,
    string Status,
    List<CartItemDto> Items,
    decimal GrandTotal
);
