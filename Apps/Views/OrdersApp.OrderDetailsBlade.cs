namespace Northwind.Apps.Views;

public class OrderDetailsBlade(long orderId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<NorthwindContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var order = this.UseState<Order?>();
        var totalAmount = this.UseState<double?>();
        var itemCount = this.UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            using var db = factory.CreateDbContext();
            var orderData = await db.Orders.Include(o => o.Customer).Include(o => o.OrderItems).SingleOrDefaultAsync(o => o.OrderId == orderId);
            order.Set(orderData);
            totalAmount.Set(orderData?.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice));
            itemCount.Set(orderData?.OrderItems.Count ?? 0);
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (order.Value == null) return null;

        var orderValue = order.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this order?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Order", AlertButtonSet.OkCancel);
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new OrderEditSheet(isOpen, refreshToken, orderId));

        var detailsCard = new Card(
            content: new
                {
                    orderValue.OrderId,
                    CustomerName = orderValue.Customer?.CompanyName,
                    orderValue.OrderDate,
                    orderValue.ShippedDate,
                    TotalAmount = totalAmount.Value
                }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.OrderId, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Order Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Order Items", onClick: _ =>
                {
                    blades.Push(this, new OrderOrderItemsBlade(orderId), "Order Items");
                }, badge: itemCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(NorthwindContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId);
        if (order != null)
        {
            db.Orders.Remove(order);
            db.SaveChanges();
        }
    }
}