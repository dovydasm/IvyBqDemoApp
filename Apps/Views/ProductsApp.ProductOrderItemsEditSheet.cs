namespace Northwind.Apps.Views;

public class ProductOrderItemsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, long orderItemId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var orderItem = UseState(() => factory.CreateDbContext().OrderItems.FirstOrDefault(e => e.OrderItemId == orderItemId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                db.OrderItems.Update(orderItem.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [orderItem]);

        return orderItem
            .ToForm()
            .Builder(e => e.UnitPrice, e => e.ToMoneyInput().Currency("USD"))
            .Place(e => e.Quantity, e => e.UnitPrice)
            .Remove(e => e.OrderItemId, e => e.OrderId, e => e.ProductId)
            .ToSheet(isOpen, "Edit Order Item");
    }
}