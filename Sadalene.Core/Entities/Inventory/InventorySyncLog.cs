namespace Sadalene.Core.Entities.Inventory;

/// <summary>
/// Audit log for every inventory sync operation (manual or scheduled).
/// </summary>
public class InventorySyncLog
{
    public int Id { get; set; }
    public DateTime SyncStartedAt { get; set; }
    public DateTime? SyncCompletedAt { get; set; }
    public bool IsSuccess { get; set; }
    public int? RecordsUpdated { get; set; }
    public string? ErrorMessage { get; set; }
    public string TriggeredBy { get; set; } = string.Empty;
}
