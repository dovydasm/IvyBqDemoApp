namespace Northwind.Apps.Views;

public class CustomerOrdersEditSheet(IState<bool> isOpen, RefreshToken refreshToken, long orderId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var order = UseState(() => factory.CreateDbContext().Orders.FirstOrDefault(e => e.OrderId == orderId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                db.Orders.Update(order.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [order]);

        return order
            .ToForm()
            .Builder(e => e.OrderDate, e => e.ToDateInput())
            .Builder(e => e.ShippedDate, e => e.ToDateInput())
            .Builder(e => e.TotalAmount, e => e.ToMoneyInput().Currency("USD"))
            .Remove(e => e.OrderId, e => e.CustomerId)
            .ToSheet(isOpen, "Edit Order");
    }
}