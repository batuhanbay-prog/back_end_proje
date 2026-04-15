using Product.Domain.Entities;

namespace Product.Application.Interfaces;

/// <summary>
/// Repository interface — veritabanı işlemlerini soyutlar.
/// </summary>
public interface IProductRepository
{
    Task<List<Product.Domain.Entities.Product>> GetAllAsync();
    Task<Product.Domain.Entities.Product?> GetByIdAsync(Guid id);
    Task AddAsync(Product.Domain.Entities.Product product);
    Task UpdateAsync(Product.Domain.Entities.Product product);
    Task DeleteAsync(Product.Domain.Entities.Product product);
    Task<List<Product.Domain.Entities.Product>> GetByCategoryAsync(string category);
}
