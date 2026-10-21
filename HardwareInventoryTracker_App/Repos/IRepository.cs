using HardwareInventoryTracker_App.Models;
using Microsoft.EntityFrameworkCore;

public interface IRepository<T> where T : class
{
    public Task<List<T>> GetAllAsync();
    public Task<T?> GetByIdAsync(int id);
    
    public Task<T> AddAsync(T item);
    public Task DeleteAsync(T item);
    public Task<T> UpdateAsync(T item);

}