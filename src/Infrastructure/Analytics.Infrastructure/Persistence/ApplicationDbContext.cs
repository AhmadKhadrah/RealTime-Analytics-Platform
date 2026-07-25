// File: src/Infrastructure/Analytics.Infrastructure/Persistence/ApplicationDbContext.cs
using Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<SystemEvent> SystemEvents => Set<SystemEvent>();
    public DbSet<HourlyAnalyticsSummary> HourlyAnalyticsSummaries => Set<HourlyAnalyticsSummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // إعدادات جدول SystemEvent
        modelBuilder.Entity<SystemEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PageUrl).HasMaxLength(500);
            entity.Property(e => e.Value).HasPrecision(18, 2);
            entity.Property(e => e.Payload).HasColumnType("nvarchar(max)");
        });

        // إعدادات جدول HourlyAnalyticsSummary
        modelBuilder.Entity<HourlyAnalyticsSummary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalValue).HasPrecision(18, 2);
            // إنشاء Index مركب لتسريع الاستعلامات بناءً على الوقت ونوع الحدث
            entity.HasIndex(e => new { e.HourUtc, e.Type }).IsUnique();
        });
    }
}