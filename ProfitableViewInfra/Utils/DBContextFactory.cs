using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProfitableViewInfra.Utils;

public class DBContextFactory : IDesignTimeDbContextFactory<DBContext>
{
    public DBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DBContext>();
        optionsBuilder.UseNpgsql("...");
        return new DBContext(optionsBuilder.Options);
    }
}