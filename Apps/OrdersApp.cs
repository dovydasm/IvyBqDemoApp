using Northwind.Apps.Views;

namespace Northwind.Apps;

[App(icon: Icons.ShoppingCart, path: ["Apps"])]
public class OrdersApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new OrderListBlade(), "Search");
    }
}