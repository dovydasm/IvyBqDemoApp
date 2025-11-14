namespace Northwind.Apps.Views;

public class OrderItemDetailsBlade(long orderItemId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var blades = UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var orderItem = UseState<OrderItem?>(() => null!);
        var (alertView, showAlert) = this.UseAlert();

        UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            orderItem.Set(await db.OrderItems
                .Include(e => e.Order)
                .Include(e => e.Product)
                .SingleOrDefaultAsync(e => e.OrderItemId == orderItemId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (orderItem.Value == null) return null;

        var orderItemValue = orderItem.Value;

        var onDelete = () =>
        {
            showAlert("Are you sure you want to delete this order item?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Order Item", AlertButtonSet.OkCancel);
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(onDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .Width(Size.Grow())
            .ToTrigger((isOpen) => new OrderItemEditSheet(isOpen, refreshToken, orderItemId));

        var detailsCard = new Card(
            content: new
            {
                OrderItemId = orderItemValue.OrderItemId,
                OrderId = orderItemValue.Order?.OrderId,
                ProductName = orderItemValue.Product?.ProductName,
                Quantity = orderItemValue.Quantity,
                UnitPrice = orderItemValue.UnitPrice,
                TotalPrice = orderItemValue.Quantity * orderItemValue.UnitPrice
            }
            .ToDetails()
            .RemoveEmpty()
            .Builder(e => e.OrderItemId, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                | dropDown
                | editBtn
        ).Title("Order Item Details");

        return new Fragment()
               | (Layout.Vertical() | detailsCard)
               | alertView;
    }

    private void Delete(NorthwindContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var orderItem = db.OrderItems.FirstOrDefault(e => e.OrderItemId == orderItemId)!;
        db.OrderItems.Remove(orderItem);
        db.SaveChanges();
    }
}