namespace Shared.Common.Events;

/// <summary>
/// Ürün güncellendiğinde fırlatılan event.
/// RabbitMQ üzerinden Log Service'e iletilir.
/// </summary>
public class ProductUpdatedEvent
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal NewPrice { get; set; }
    public int NewStock { get; set; }
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    public string UpdatedBy { get; set; } = string.Empty;
}
