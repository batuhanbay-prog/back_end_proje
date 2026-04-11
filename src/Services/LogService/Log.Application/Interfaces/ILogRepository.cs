using Log.Domain.Entities;

namespace Log.Application.Interfaces;

/// <summary>
/// Log repository interface — log kayıtlarına erişim soyutlaması.
/// </summary>
public interface ILogRepository
{
    Task<List<LogEntry>> GetAllAsync(string? serviceName, string? logLevel, DateTime? from, DateTime? to);
    Task<LogEntry?> GetByIdAsync(Guid id);
    Task AddAsync(LogEntry logEntry);
}
