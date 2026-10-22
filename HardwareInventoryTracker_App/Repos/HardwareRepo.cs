
using HardwareInventoryTracker_App.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

class HardwareItemRepo : IRepository<HardwareItem>
{
    private readonly IDbContextFactory<HardwareInventoryManagerContext> _dbFactory;

    public HardwareItemRepo(IDbContextFactory<HardwareInventoryManagerContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }
    public async Task<List<HardwareItem>> GetAllAsync()
    {
        using var dbContext = _dbFactory.CreateDbContext();
        return await dbContext.HardwareItems.Include(o => o.Category).Include(o => o.OrderHistories).ToListAsync();
    }

    public async Task<HardwareItem?> GetByIdAsync(int id)
    {
        using var dbContext = _dbFactory.CreateDbContext();
        return await dbContext.HardwareItems.Include(o => o.OrderHistories).Include(c => c.Category).Where(i => i.Id == id).FirstOrDefaultAsync();
    }

    public async Task DeleteAsync(HardwareItem item)
    {
        using (var dbContext = _dbFactory.CreateDbContext())
        {
            dbContext.HardwareItems.Remove(item);
            await dbContext.SaveChangesAsync();
            //does this delete all related order histories?            
        }

    }

    public async Task<HardwareItem> UpdateAsync(HardwareItem item)
    {
        using var dbContext = _dbFactory.CreateDbContext();
        var existingItem = await dbContext.HardwareItems.FirstOrDefaultAsync(i => i.Id == item.Id);
        if (existingItem is null)
        {
            throw new KeyNotFoundException($"Hardware item with ID {item.Id} was not found.");
        }

        existingItem.Name = item.Name;
        existingItem.Description = item.Description;
        existingItem.CategoryId = item.CategoryId;
        existingItem.PackageQuantitiy = item.PackageQuantitiy;
        existingItem.SuggestedOrder = item.SuggestedOrder;
        existingItem.ReorderThreshold = item.ReorderThreshold;
        existingItem.ReorderLink = item.ReorderLink;
        existingItem.ProductImageUrl = item.ProductImageUrl;
        existingItem.Quantity = item.Quantity;
        await dbContext.SaveChangesAsync();
        return existingItem;

    }

    public async Task<HardwareItem> AddAsync(HardwareItem item)
    {
        using var dbContext = _dbFactory.CreateDbContext();
        dbContext.HardwareItems.Add(item);

        await dbContext.SaveChangesAsync();

        return item;
    }

    public async Task DecreaseQuantity(int hardwareItemId, int quantity)
    {

        using (var dbContext = _dbFactory.CreateDbContext())
        {
            var item = await dbContext.HardwareItems.FirstOrDefaultAsync(i => i.Id == hardwareItemId);
            if (item is not null)
            {
                item.Quantity = item.Quantity - quantity;
                await dbContext.SaveChangesAsync();
            }
        }
    }
}