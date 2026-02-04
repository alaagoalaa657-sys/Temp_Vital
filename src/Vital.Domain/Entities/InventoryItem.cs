namespace Vital.Domain.Entities;

public class InventoryItem
{
    public int InventoryItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public ItemCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinimumQuantity { get; set; } = 10;
    public decimal UnitPrice { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}

public enum ItemCategory
{
    Medicine = 1,
    Equipment = 2,
    Supplies = 3,
    Consumable = 4,
    Other = 5
}
