using System.Reflection;
using Ivy.EntityFrameworkCore.BigQuery.Extensions;

namespace Northwind.Connections.Northwind;

public class NorthwindContextFactory(ServerArgs args) : IDbContextFactory<NorthwindContext>
{
    public NorthwindContext CreateDbContext()
    {
        var configuration = new ConfigurationBuilder()
           .AddEnvironmentVariables()
           .AddUserSecrets(Assembly.GetExecutingAssembly())
           .Build();

        var optionsBuilder = new DbContextOptionsBuilder<NorthwindContext>();

        var connectionString = configuration.GetConnectionString("Northwind");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string 'Northwind' is not set.");
        }

        optionsBuilder.UseBigQuery(connectionString);

        if (args.Verbose)
        {
            optionsBuilder
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information);
        }

        return new NorthwindContext(optionsBuilder.Options);
    }
}
