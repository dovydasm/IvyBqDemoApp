using Northwind.Apps.Views;

namespace Northwind.Apps;

[App(icon: Icons.ShoppingCart, path: ["Apps"])]
public class OrderItemsApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new OrderItemListBlade(), "Search");
    }
}