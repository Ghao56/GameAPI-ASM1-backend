using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GameApi.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Set ConnectionStrings:Default in appsettings.json or ConnectionStrings__Default in the environment.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(connectionString, AppDbContext.ServerVersion)
            .Options;

        return new AppDbContext(options);
    }
}
