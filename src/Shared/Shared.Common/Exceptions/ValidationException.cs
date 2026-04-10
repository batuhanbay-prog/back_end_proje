using FluentValidation.Results;

namespace Shared.Common.Exceptions;

/// <summary>
/// Doğrulama hatası olduğunda fırlatılır. HTTP 400 döner.
/// FluentValidation ile entegre çalışır.
/// </summary>
public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException() : base("Bir veya daha fazla doğrulama hatası oluştu.")
    {
        Errors = new List<string>();
    }

    public ValidationException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }

    public ValidationException(List<ValidationFailure> failures) : this()
    {
        Errors = failures.Select(f => f.ErrorMessage).ToList();
    }
}
