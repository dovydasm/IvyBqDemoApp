namespace Northwind.Apps.Views;

public class ProductOrderItemsBlade(long? productId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var orderItems = this.UseState<OrderItem[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            await using var db = factory.CreateDbContext();
            orderItems.Set(await db.OrderItems
                .Include(e => e.Order)
                .Where(e => e.ProductId == productId)
                .ToArrayAsync());
        }, [ EffectTrigger.AfterInit(), refreshToken ]);

        Action OnDelete(long id)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this order item?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, id);
                        refreshToken.Refresh();
                    }
                }, "Delete Order Item", AlertButtonSet.OkCancel);
            };
        }

        if (orderItems.Value == null) return null;

        var table = orderItems.Value.Select(e => new
            {
                OrderDate = e.Order?.OrderDate,
                ShippedDate = e.Order?.ShippedDate,
                Quantity = e.Quantity,
                UnitPrice = e.UnitPrice,
                TotalPrice = e.Quantity * e.UnitPrice,
                _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(e.OrderItemId)))
                    | Icons.Pencil
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new ProductOrderItemsEditSheet(isOpen, refreshToken, e.OrderItemId))
            })
            .ToTable()
            .Totals(e => e.TotalPrice)
            .Totals(e => e.Quantity)
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Order Item").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new ProductOrderItemsCreateDialog(isOpen, refreshToken, productId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(NorthwindContextFactory factory, long orderItemId)
    {
        using var db = factory.CreateDbContext();
        db.OrderItems.Remove(db.OrderItems.Single(e => e.OrderItemId == orderItemId));
        db.SaveChanges();
    }
}