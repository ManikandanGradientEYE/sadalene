namespace Sadalene.API.DTOs.Catalog;

public record ProductListItemDto(
    int Id,
    string Name,
    string? ProductCode,
    string? MarketName,
    decimal? Rate,
    string Uom,
    decimal Stock,
    string? PrimaryImageUrl,
    int DivisionId,
    string DivisionName
);
