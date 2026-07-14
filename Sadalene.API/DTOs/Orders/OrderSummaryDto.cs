namespace Sadalene.API.DTOs.Orders;

public record OrderSummaryDto(
    int Id,
    string OrderNumber,
    string CustomerName,
    DateTime OrderDate,
    string Status,
    int ItemCount,
    decimal GrandTotal
);
