namespace Sadalene.API.DTOs.Catalog;

public record ProductDetailDto(
    int Id,
    string Name,
    string? MarketName,
    string? Description,
    string? ProductCode,
    int DivisionId,
    string DivisionName,
    int CategoryId,
    string CategoryName,
    int SubCategoryId,
    string SubCategoryName,
    decimal? Rate,
    string? RatePer,
    string Uom,
    bool AllowsHalfUnit,
    decimal Stock,
    string? Grade,
    string? FabricComposition,
    string? Width,
    string? Weight,
    string? Color,
    string? DesignNo,
    string? Design,
    string? Brand,
    List<string> ImageUrls
);
