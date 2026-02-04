using Vital.Domain.Entities;
using Vital.Repository.Interfaces;

namespace Vital.BusinessLogic.Managers;

/// <summary>
/// Business logic for inventory management
/// </summary>
public class InventoryManager
{
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryManager(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
    }

    /// <summary>
    /// Gets all inventory items
    /// </summary>
    /// <returns>Collection of all inventory items</returns>
    public async Task<IEnumerable<InventoryItem>> GetAllItemsAsync()
    {
        return await _inventoryRepository.GetAllAsync();
    }

    /// <summary>
    /// Gets inventory items with low stock (below minimum quantity)
    /// </summary>
    /// <returns>Collection of low stock items</returns>
    public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync()
    {
        return await _inventoryRepository.GetLowStockItemsAsync();
    }

    /// <summary>
    /// Gets inventory items expiring within the specified number of days
    /// </summary>
    /// <param name="daysAhead">Number of days to look ahead</param>
    /// <returns>Collection of expiring items</returns>
    /// <exception cref="ArgumentException">Thrown when daysAhead is invalid</exception>
    public async Task<IEnumerable<InventoryItem>> GetExpiringItemsAsync(int daysAhead = 30)
    {
        if (daysAhead <= 0)
            throw new ArgumentException("Days ahead must be greater than zero", nameof(daysAhead));

        return await _inventoryRepository.GetExpiringItemsAsync(daysAhead);
    }

    /// <summary>
    /// Creates a new inventory item
    /// </summary>
    /// <param name="item">Inventory item entity</param>
    /// <returns>Created inventory item with ID</returns>
    /// <exception cref="ArgumentNullException">Thrown when item is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public async Task<InventoryItem> CreateItemAsync(InventoryItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        ValidateInventoryItem(item);

        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = null;

        return await _inventoryRepository.CreateAsync(item);
    }

    /// <summary>
    /// Updates an existing inventory item
    /// </summary>
    /// <param name="item">Inventory item entity with updated data</param>
    /// <returns>Updated inventory item</returns>
    /// <exception cref="ArgumentNullException">Thrown when item is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    /// <exception cref="InvalidOperationException">Thrown when item does not exist</exception>
    public async Task<InventoryItem> UpdateItemAsync(InventoryItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (item.InventoryItemId <= 0)
            throw new ArgumentException("Inventory item ID must be greater than zero", nameof(item.InventoryItemId));

        ValidateInventoryItem(item);

        var existingItem = await _inventoryRepository.GetByIdAsync(item.InventoryItemId);
        if (existingItem == null)
            throw new InvalidOperationException($"Inventory item with ID {item.InventoryItemId} does not exist");

        item.UpdatedAt = DateTime.UtcNow;

        return await _inventoryRepository.UpdateAsync(item);
    }

    /// <summary>
    /// Deletes an inventory item
    /// </summary>
    /// <param name="itemId">Inventory item ID</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when itemId is invalid</exception>
    public async Task<bool> DeleteItemAsync(int itemId)
    {
        if (itemId <= 0)
            throw new ArgumentException("Inventory item ID must be greater than zero", nameof(itemId));

        return await _inventoryRepository.DeleteAsync(itemId);
    }

    /// <summary>
    /// Updates the stock quantity of an inventory item
    /// </summary>
    /// <param name="itemId">Inventory item ID</param>
    /// <param name="quantityChange">Quantity to add (positive) or subtract (negative)</param>
    /// <returns>True if update was successful, false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when itemId is invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when item does not exist or update would result in negative quantity</exception>
    public async Task<bool> UpdateStockAsync(int itemId, int quantityChange)
    {
        if (itemId <= 0)
            throw new ArgumentException("Inventory item ID must be greater than zero", nameof(itemId));

        var item = await _inventoryRepository.GetByIdAsync(itemId);
        if (item == null)
            throw new InvalidOperationException($"Inventory item with ID {itemId} does not exist");

        var newQuantity = item.Quantity + quantityChange;
        if (newQuantity < 0)
            throw new InvalidOperationException(
                $"Cannot update stock. Current quantity: {item.Quantity}, Change: {quantityChange}. Result would be negative.");

        return await _inventoryRepository.UpdateQuantityAsync(itemId, quantityChange);
    }

    private void ValidateInventoryItem(InventoryItem item)
    {
        if (string.IsNullOrWhiteSpace(item.ItemName))
            throw new ArgumentException("Item name is required", nameof(item.ItemName));

        if (string.IsNullOrWhiteSpace(item.ItemCode))
            throw new ArgumentException("Item code is required", nameof(item.ItemCode));

        if (item.Quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(item.Quantity));

        if (item.MinimumQuantity < 0)
            throw new ArgumentException("Minimum quantity cannot be negative", nameof(item.MinimumQuantity));

        if (item.UnitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(item.UnitPrice));

        if (string.IsNullOrWhiteSpace(item.Unit))
            throw new ArgumentException("Unit is required", nameof(item.Unit));

        if (item.ExpiryDate.HasValue && item.ExpiryDate.Value < DateTime.Today)
            throw new ArgumentException("Expiry date cannot be in the past", nameof(item.ExpiryDate));
    }
}
