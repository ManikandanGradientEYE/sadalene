using Sadalene.Core.Common;

namespace Sadalene.Core.Entities.Masters;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;

    public ICollection<SubCategory> SubCategories { get; set; } = [];
}
