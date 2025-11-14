using Northwind.Apps.Views;

namespace Northwind.Apps;

[App(icon: Icons.Folder, path: ["Apps"])]
public class CategoriesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new CategoryListBlade(), "Search");
    }
}