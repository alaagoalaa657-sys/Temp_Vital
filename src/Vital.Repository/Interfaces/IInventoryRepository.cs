using Vital.Domain.Entities;

namespace Vital.Repository.Interfaces;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByIdAsync(int itemId);
    Task<IEnumerable<InventoryItem>> GetAllAsync();
    Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync();
    Task<IEnumerable<InventoryItem>> GetExpiringItemsAsync(int daysAhead);
    Task<IEnumerable<InventoryItem>> SearchAsync(string searchTerm);
    Task<InventoryItem> CreateAsync(InventoryItem item);
    Task<InventoryItem> UpdateAsync(InventoryItem item);
    Task<bool> DeleteAsync(int itemId);
    Task<bool> UpdateQuantityAsync(int itemId, int quantityChange);
}
