namespace Sadalene.API.DTOs.Orders;

public record OrderItemDto(
    int ProductId,
    string ProductName,
    string? ProductCode,
    decimal Quantity,
    string UnitType,
    string UnitOfMeasure,
    decimal EffectiveQuantity,
    decimal UnitPrice,
    decimal LineTotal
);
