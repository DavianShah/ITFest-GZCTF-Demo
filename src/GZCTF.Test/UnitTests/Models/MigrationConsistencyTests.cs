using GZCTF.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GZCTF.Test.UnitTests.Models;

public class MigrationConsistencyTests
{
    [Fact]
    public void CurrentModel_MatchesMigrationSnapshot()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model_check;Username=postgres;Password=postgres")
            .Options;

        using var context = new AppDbContext(options);

        Assert.False(context.Database.HasPendingModelChanges());
    }
}
