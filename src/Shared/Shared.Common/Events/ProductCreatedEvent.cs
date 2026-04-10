namespace Shared.Common.Events;

/// <summary>
/// Ürün oluşturulduğunda fırlatılan event.
/// RabbitMQ üzerinden Log Service'e iletilir.
/// </summary>
public class ProductCreatedEvent
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}
