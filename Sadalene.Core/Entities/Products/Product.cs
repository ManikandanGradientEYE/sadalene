using Sadalene.Core.Common;
using Sadalene.Core.Entities.Masters;
using Sadalene.Core.Entities.Inventory;
using Sadalene.Core.Entities.Orders;

namespace Sadalene.Core.Entities.Products;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? MarketName { get; set; }
    public string? Description { get; set; }
    public string? ProductCode { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int SubCategoryId { get; set; }
    public SubCategory SubCategory { get; set; } = null!;

    public int ProductTypeId { get; set; }
    public ProductType ProductType { get; set; } = null!;

    public int DivisionId { get; set; }
    public Division Division { get; set; } = null!;

    // Pricing & UOM
    public int? UomMasterId { get; set; }
    public UomMaster? UomMaster { get; set; }
    public decimal? Rate { get; set; }
    public string? RatePer { get; set; }
    public decimal? Cut { get; set; }
    public decimal? QtyPerUnit { get; set; }
    public int? PackingTypeId { get; set; }
    public PackingType? PackingType { get; set; }
    public string? Grade { get; set; }

    // Specifications
    public string? FabricComposition { get; set; }
    public string? Width { get; set; }
    public string? Weight { get; set; }
    public string? Color { get; set; }
    public string? DesignNo { get; set; }
    public string? Design { get; set; }
    public string? Brand { get; set; }

    // Barcode
    public string? BarcodeValue { get; set; }
    public string? BarcodeImageUrl { get; set; }
    public bool IsBarcodeActive { get; set; } = false;
    public DateTime? BarcodeGeneratedAt { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<InventoryRecord> InventoryRecords { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
