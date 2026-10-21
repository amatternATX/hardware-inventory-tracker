
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
        return await dbContext.HardwareItems.Include(o => o.OrderHistories).Where(i => i.Id == id).FirstOrDefaultAsync();
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
        await dbContext.HardwareItems.AddAsync(item);
        await dbContext.SaveChangesAsync();
        return item;

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