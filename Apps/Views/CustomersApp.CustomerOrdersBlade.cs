namespace Northwind.Apps.Views;

public class CustomerOrdersBlade(long? customerId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var orders = this.UseState<Order[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            if (customerId.HasValue)
            {
                orders.Set(await db.Orders
                    .Where(o => o.CustomerId == customerId)
                    .ToArrayAsync());
            }
        }, [EffectTrigger.AfterInit(), refreshToken]);

        Action OnDelete(long orderId)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this order?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, orderId);
                        refreshToken.Refresh();
                    }
                }, "Delete Order", AlertButtonSet.OkCancel);
            };
        }

        if (orders.Value == null) return null;

        var table = orders.Value.Select(o => new
            {
                OrderDate = o.OrderDate,
                ShippedDate = o.ShippedDate,
                TotalAmount = o.TotalAmount,
                _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(o.OrderId)))
                    | Icons.Pencil
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new CustomerOrdersEditSheet(isOpen, refreshToken, o.OrderId))
            })
            .ToTable()
            .Totals(e => e.TotalAmount)
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Order").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new CustomerOrdersCreateDialog(isOpen, refreshToken, customerId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(NorthwindContextFactory factory, long orderId)
    {
        using var db = factory.CreateDbContext();
        var order = db.Orders.Single(o => o.OrderId == orderId);
        db.Orders.Remove(order);
        db.SaveChanges();
    }
}