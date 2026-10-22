using HardwareInventoryTracker_App.Models;
using Microsoft.EntityFrameworkCore;

public class CategoryRepo : IRepository<Category>
{
    private readonly IDbContextFactory<HardwareInventoryManagerContext> _dbFactory;

    public CategoryRepo(IDbContextFactory<HardwareInventoryManagerContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        using var dbContext = _dbFactory.CreateDbContext();
        return await dbContext.Categories.OrderBy(category => category.CategoryName).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        using var dbContext = _dbFactory.CreateDbContext();
        return await dbContext.Categories.FirstOrDefaultAsync(category => category.Id == id);
    }

    public async Task<Category> AddAsync(Category item)
    {
        using var dbContext = _dbFactory.CreateDbContext();
        await dbContext.Categories.AddAsync(item);
        await dbContext.SaveChangesAsync();
        return item;
    }

    public async Task DeleteAsync(Category item)
    {
        using var dbContext = _dbFactory.CreateDbContext();
        dbContext.Categories.Remove(item);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Category> UpdateAsync(Category item)
    {
        using var dbContext = _dbFactory.CreateDbContext();
        dbContext.Categories.Update(item);
        await dbContext.SaveChangesAsync();
        return item;
    }
}