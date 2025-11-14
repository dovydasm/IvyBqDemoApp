using Northwind.Apps.Views;

namespace Northwind.Apps;

[App(icon: Icons.User, path: ["Apps"])]
public class CustomersApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new CustomerListBlade(), "Search");
    }
}