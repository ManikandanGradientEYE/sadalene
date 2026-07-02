using Sadalene.Core.Common;
using Sadalene.Core.Entities.Products;

namespace Sadalene.Core.Entities.Masters;

public class SubCategory : BaseEntity
{
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;

    public ICollection<Product> Products { get; set; } = [];
}
