namespace Sadalene.API.DTOs.Orders;

public record OrderDetailDto(
    int Id,
    string OrderNumber,
    string CustomerName,
    string? AgentName,
    string? PlacedByName,
    DateTime OrderDate,
    string Status,
    string? Notes,
    List<OrderItemDto> Items,
    decimal GrandTotal
);
