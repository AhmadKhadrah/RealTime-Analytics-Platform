// File: src/Infrastructure/Analytics.Infrastructure/Persistence/ApplicationDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Analytics.Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // نص اتصال افتراضي للمحلي (Localhost) يُستخدم فقط أثناء توليد الـ Migrations في الـ Terminal
        var connectionString = "Server=.;Database=AnalyticsDb;User Id=sa2;Password=Allah1111#;TrustServerCertificate=True;";
        
        optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}