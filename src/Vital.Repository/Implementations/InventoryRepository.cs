using Microsoft.EntityFrameworkCore;
using Vital.Domain.Entities;
using Vital.Repository.Data;
using Vital.Repository.Interfaces;

namespace Vital.Repository.Implementations;

public class InventoryRepository : IInventoryRepository
{
    private readonly VitalDbContext _context;

    public InventoryRepository(VitalDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryItem?> GetByIdAsync(int itemId)
    {
        return await _context.InventoryItems
            .Include(i => i.PrescriptionItems)
            .FirstOrDefaultAsync(i => i.InventoryItemId == itemId);
    }

    public async Task<IEnumerable<InventoryItem>> GetAllAsync()
    {
        return await _context.InventoryItems
            .OrderBy(i => i.ItemName)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync()
    {
        return await _context.InventoryItems
            .Where(i => i.Quantity <= i.MinimumQuantity)
            .OrderBy(i => i.Quantity)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryItem>> GetExpiringItemsAsync(int daysAhead)
    {
        var targetDate = DateTime.UtcNow.AddDays(daysAhead);
        return await _context.InventoryItems
            .Where(i => i.ExpiryDate.HasValue && 
                       i.ExpiryDate.Value <= targetDate &&
                       i.ExpiryDate.Value >= DateTime.UtcNow)
            .OrderBy(i => i.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryItem>> SearchAsync(string searchTerm)
    {
        return await _context.InventoryItems
            .Where(i => i.ItemName.Contains(searchTerm) || 
                       i.ItemCode.Contains(searchTerm) ||
                       i.Description.Contains(searchTerm))
            .OrderBy(i => i.ItemName)
            .ToListAsync();
    }

    public async Task<InventoryItem> CreateAsync(InventoryItem item)
    {
        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<InventoryItem> UpdateAsync(InventoryItem item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteAsync(int itemId)
    {
        var item = await _context.InventoryItems.FindAsync(itemId);
        if (item == null) return false;

        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateQuantityAsync(int itemId, int quantityChange)
    {
        var item = await _context.InventoryItems.FindAsync(itemId);
        if (item == null) return false;

        item.Quantity += quantityChange;
        item.UpdatedAt = DateTime.UtcNow;
        
        _context.InventoryItems.Update(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
