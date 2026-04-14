namespace UserManagement.Domain.Interfaces
{
    /// <summary>
    /// Generic repository interface defining standard CRUD operations.
    /// All specific repository interfaces inherit from this, ensuring
    /// consistency and avoiding code duplication (DRY principle).
    /// </summary>
    /// <typeparam name="T">The entity type this repository manages.</typeparam>
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}