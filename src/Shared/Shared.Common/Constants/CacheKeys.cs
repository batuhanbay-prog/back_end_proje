namespace Shared.Common.Constants;

/// <summary>
/// Redis cache key sabitleri.
/// Tüm servisler aynı key formatını kullanır.
/// </summary>
public static class CacheKeys
{
    public const string AllProducts = "products_all";
    public static string ProductById(Guid id) => $"product_{id}";
}
