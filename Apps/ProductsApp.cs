using Northwind.Apps.Views;

namespace Northwind.Apps;

[App(icon: Icons.Package, path: ["Apps"])]
public class ProductsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new ProductListBlade(), "Search");
    }
}