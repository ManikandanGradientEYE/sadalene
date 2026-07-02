using Sadalene.Core.Common;
using Sadalene.Core.Entities.Products;

namespace Sadalene.Core.Entities.Masters;

public class ProductType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = [];
}
