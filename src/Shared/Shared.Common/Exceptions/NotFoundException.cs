namespace Shared.Common.Exceptions;

/// <summary>
/// Kayıt bulunamadığında fırlatılır. HTTP 404 döner.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException() : base("Kayıt bulunamadı.") { }
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string name, object key) : base($"{name} ({key}) bulunamadı.") { }
    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
