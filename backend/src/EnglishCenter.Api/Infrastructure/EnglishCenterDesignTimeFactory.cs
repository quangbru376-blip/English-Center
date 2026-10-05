namespace EnglishCenter.Api.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class EnglishCenterDesignTimeFactory : IDesignTimeDbContextFactory<EnglishCenterDbContext>
{
    public EnglishCenterDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<EnglishCenterDbContext>()
            .UseSqlServer("Server=disconnected.invalid;Database=DesignOnly;Integrated Security=True;Encrypt=True",
                sql => sql.UseCompatibilityLevel(160))
            .Options;
        return new EnglishCenterDbContext(options);
    }
}
