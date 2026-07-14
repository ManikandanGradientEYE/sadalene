namespace Sadalene.API.DTOs.Cart;

public record AddCartItemRequest(
    int ProductId,
    decimal Quantity,
    string UnitType = "Full",
    bool AddedByBarcodeScan = false,
    string? ScannedBarcodeValue = null
);
