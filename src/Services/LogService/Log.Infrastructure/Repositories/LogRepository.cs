using Log.Application.Interfaces;
using Log.Domain.Entities;
using Log.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Log.Infrastructure.Repositories;

/// <summary>
/// ILogRepository implementasyonu — EF Core ile log CRUD işlemleri.
/// </summary>
public class LogRepository : ILogRepository
{
    private readonly LogDbContext _context;

    public LogRepository(LogDbContext context)
    {
        _context = context;
    }

    public async Task<List<LogEntry>> GetAllAsync(string? serviceName, string? logLevel, DateTime? from, DateTime? to)
    {
        var query = _context.LogEntries.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(serviceName))
            query = query.Where(l => l.ServiceName == serviceName);

        if (!string.IsNullOrEmpty(logLevel))
            query = query.Where(l => l.LogLevel == logLevel);

        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);

        return await query.OrderByDescending(l => l.Timestamp).ToListAsync();
    }

    public async Task<LogEntry?> GetByIdAsync(Guid id)
    {
        return await _context.LogEntries.FindAsync(id);
    }

    public async Task AddAsync(LogEntry logEntry)
    {
        await _context.LogEntries.AddAsync(logEntry);
        await _context.SaveChangesAsync();
    }
}
