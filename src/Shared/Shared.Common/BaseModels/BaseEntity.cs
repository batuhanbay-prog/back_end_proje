namespace Shared.Common.BaseModels;

/// <summary>
/// Tüm entity'lerin türeyeceği base class.
/// Id, CreatedDate, UpdatedDate alanlarını içerir.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
}
