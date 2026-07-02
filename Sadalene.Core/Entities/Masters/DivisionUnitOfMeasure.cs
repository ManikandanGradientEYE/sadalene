using Sadalene.Core.Common;

namespace Sadalene.Core.Entities.Masters;

/// <summary>
/// Maps a division to its valid units of measure.
/// A division can have multiple UOMs (e.g. Lump has Full Set and Half Set).
/// </summary>
public class DivisionUnitOfMeasure : BaseEntity
{
    public int DivisionId { get; set; }
    public Division Division { get; set; } = null!;

    public string UnitName { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
}
