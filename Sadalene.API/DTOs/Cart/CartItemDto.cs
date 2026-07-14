namespace Sadalene.API.DTOs.Cart;

public record CartItemDto(
    int Id,
    int ProductId,
    string ProductName,
    string? ProductCode,
    decimal Quantity,
    string UnitType,
    string UnitOfMeasure,
    decimal EffectiveQuantity,
    decimal DisplayUnitPrice,
    decimal LineTotal,
    bool AddedByBarcodeScan
);
