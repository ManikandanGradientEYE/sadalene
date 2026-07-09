using Sadalene.Core.Common;
using Sadalene.Core.Entities.Products;

namespace Sadalene.Core.Entities.Masters;

public class Division : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string Description { get; set; } = string.Empty;

    public ICollection<Category> Categories { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
