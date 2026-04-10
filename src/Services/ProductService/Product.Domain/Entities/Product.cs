using Shared.Common.BaseModels;

namespace Product.Domain.Entities;

/// <summary>
/// Ürün entity'si. BaseEntity'den türer (Id, CreatedDate, UpdatedDate).
/// </summary>
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
