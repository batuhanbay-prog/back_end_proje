using Log.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Log.Infrastructure.Data;

/// <summary>
/// Log mikroservisi veritabanı context'i.
/// </summary>
public class LogDbContext : DbContext
{
    public LogDbContext(DbContextOptions<LogDbContext> options) : base(options)
    {
    }

    public DbSet<LogEntry> LogEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LogLevel).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.Exception).HasMaxLength(8000);
            entity.Property(e => e.CorrelationId).HasMaxLength(100);
            entity.HasIndex(e => e.LogLevel);
            entity.HasIndex(e => e.ServiceName);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
