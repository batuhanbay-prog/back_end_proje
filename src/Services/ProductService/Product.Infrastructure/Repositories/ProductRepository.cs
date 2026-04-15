using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Infrastructure.Data;

namespace Product.Infrastructure.Repositories;

/// <summary>
/// IProductRepository implementasyonu — EF Core ile veritabanı işlemleri.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product.Domain.Entities.Product>> GetAllAsync()
    {
        return await _context.Products.AsNoTracking().ToListAsync();
    }

    public async Task<Product.Domain.Entities.Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task AddAsync(Product.Domain.Entities.Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product.Domain.Entities.Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product.Domain.Entities.Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// T-SQL Raw Query örneği — kategoriye göre ürün listesi.
    /// PDF gereksinimi: "Programlama Dili: C#, Transact-SQL"
    /// </summary>
    public async Task<List<Product.Domain.Entities.Product>> GetByCategoryAsync(string category)
    {
        var param = new SqlParameter("@category", category);
        return await _context.Products
            .FromSqlRaw("SELECT * FROM Products WHERE Category = @category AND IsActive = 1", param)
            .AsNoTracking()
            .ToListAsync();
    }
}
