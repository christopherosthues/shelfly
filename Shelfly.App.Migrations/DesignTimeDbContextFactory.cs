using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Shelfly.App.Data;

namespace Shelfly.App.Migrations;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LocalDbContext>
{
    public LocalDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<LocalDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlite("Data Source=shelfly.db",
            sql => sql.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));
        return new LocalDbContext(optionsBuilder.Options);
    }
}