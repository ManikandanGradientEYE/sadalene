using Sadalene.Core.Common;
using Sadalene.Core.Entities.Products;

namespace Sadalene.Core.Entities.Masters;

/// <summary>
/// Textile division determines the unit of measure used in ordering.
/// Lump=Full Set/Half Set | Cutpack=Taka | Pieces=No. of Boxes | Make to Order=Meters
/// </summary>
public class Division : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<DivisionUnitOfMeasure> UnitOfMeasures { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
