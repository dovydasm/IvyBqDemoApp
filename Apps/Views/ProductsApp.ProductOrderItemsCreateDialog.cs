namespace Northwind.Apps.Views;

public class ProductOrderItemsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, long? productId) : ViewBase
{
    private record OrderItemCreateRequest
    {
        [Required]
        public long Quantity { get; init; }

        [Required]
        public double UnitPrice { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var orderItemState = UseState(() => new OrderItemCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                CreateOrderItem(factory, orderItemState.Value);
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [orderItemState]);

        return orderItemState
            .ToForm()
            .Builder(e => e.UnitPrice, e => e.ToMoneyInput().Currency("USD"))
            .ToDialog(isOpen, title: "Create Order Item", submitTitle: "Create");
    }

    private void CreateOrderItem(NorthwindContextFactory factory, OrderItemCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var orderItem = new OrderItem
        {
            ProductId = productId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice
        };

        db.OrderItems.Add(orderItem);
        db.SaveChanges();
    }
}