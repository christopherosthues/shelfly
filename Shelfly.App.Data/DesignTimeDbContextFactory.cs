using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Shelfly.App.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LocalDbContext>
{
    public LocalDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<LocalDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlite("Data Source=shelfly.db");
        return new LocalDbContext(optionsBuilder.Options);
    }
}