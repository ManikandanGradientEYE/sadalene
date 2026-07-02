using Sadalene.Core.Common;
using Sadalene.Core.Entities.Products;

namespace Sadalene.Core.Entities.Masters;

public class UomMaster : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Abbreviation { get; set; }
    public string? Description { get; set; }

    public ICollection<SubCategory> SubCategories { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
