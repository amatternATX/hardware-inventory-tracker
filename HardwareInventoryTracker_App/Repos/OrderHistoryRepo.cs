using HardwareInventoryTracker_App.Models;
using Microsoft.EntityFrameworkCore;

public class OrderHistoryRepo : IRepository<OrderHistory>
{
    private readonly IDbContextFactory<HardwareInventoryManagerContext> _dbFactory;
    public OrderHistoryRepo(IDbContextFactory<HardwareInventoryManagerContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<OrderHistory> AddAsync(OrderHistory item)
    {
        using (var dbContext = _dbFactory.CreateDbContext())
        {
            await dbContext.OrderHistories.AddAsync(item);
            await dbContext.SaveChangesAsync();
        }

        return item;        
    }

    public Task DeleteAsync(OrderHistory item)
    {
        throw new NotImplementedException();
    }

    public async Task<List<OrderHistory>> GetAllAsync()
    {
        using var dbContext = _dbFactory.CreateDbContext();
        return await dbContext.OrderHistories.Include(o => o.HardwareItem).ToListAsync();
    }

    public async Task<OrderHistory?> GetByIdAsync(int id)
    {
        using var dbContext = _dbFactory.CreateDbContext();
        return await dbContext.OrderHistories.Include(o => o.HardwareItem).Where(i => i.Id == id).FirstOrDefaultAsync();        
    }

    public Task<OrderHistory> UpdateAsync(OrderHistory item)
    {
        throw new NotImplementedException();
    }

    public async Task FulFillOrder(int id)
    {
        using (var dbContext = _dbFactory.CreateDbContext())
        {
            var history = await dbContext.OrderHistories.Include(o => o.HardwareItem).FirstOrDefaultAsync(o => o.Id == id);
            if(history is not null)
            {
                history.FulfillmentDate = DateOnly.FromDateTime(DateTime.Now);
                history.HardwareItem.Quantity += history.OrderQuantity * history.HardwareItem.PackageQuantitiy;
                await dbContext.SaveChangesAsync();
            }
        }
    }
}