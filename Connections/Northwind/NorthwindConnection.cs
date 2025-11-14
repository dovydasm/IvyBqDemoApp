using Ivy.Connections;
using Ivy.Services;

namespace Northwind.Connections.Northwind;

public class NorthwindConnection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath)
    {
        var connectionFile = nameof(NorthwindConnection) + ".cs";
        var contextFactoryFile = nameof(NorthwindContextFactory) + ".cs";
        var files = System.IO.Directory.GetFiles(connectionPath, "*.*", System.IO.SearchOption.TopDirectoryOnly)
            .Where(f => !f.EndsWith(connectionFile) && !f.EndsWith(contextFactoryFile) && !f.EndsWith("EfmigrationsLock.cs"))
            .Select(System.IO.File.ReadAllText)
            .ToArray();
        return string.Join(System.Environment.NewLine, files);
    }

    public string GetName() => nameof(Northwind);

    public string GetNamespace() => typeof(NorthwindConnection).Namespace;

    public string GetConnectionType() => "EntityFramework.BigQuery";

    public ConnectionEntity[] GetEntities()
    {
        return typeof(NorthwindContext)
            .GetProperties()
            .Where(e => e.PropertyType.IsGenericType && e.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Where(e => e.PropertyType.GenericTypeArguments[0].Name != "EfmigrationsLock")
            .Select(e => new ConnectionEntity(e.PropertyType.GenericTypeArguments[0].Name, e.Name))
            .ToArray();
    }

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<NorthwindContextFactory>();
    }

   public Ivy.Services.Secret[] GetSecrets()
   {
       return
       [
           new("ConnectionStrings:Northwind")
       ];
   }
}
