using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Shelfly.Api.Data.Migrations;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShelflyDbContext>
{
    public ShelflyDbContext CreateDbContext(string[] args)
    {
        string connectionString = "Host=localhost;Port=5432;Database=shelfly;Username=postgres;Password=postgres";

        DbContextOptionsBuilder<ShelflyDbContext> optionsBuilder = new();
        optionsBuilder.UseNpgsql(connectionString);

        return new ShelflyDbContext(optionsBuilder.Options);
    }
}
