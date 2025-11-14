namespace Northwind.Apps.Views;

public class CustomerOrdersCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, long? customerId) : ViewBase
{
    private record OrderCreateRequest
    {
        [Required]
        public DateOnly OrderDate { get; init; }

        public DateOnly? ShippedDate { get; init; }

        public double? TotalAmount { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var order = UseState(() => new OrderCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                var orderId = CreateOrder(factory, order.Value);
                refreshToken.Refresh(orderId);
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [order]);

        return order
            .ToForm()
            .Builder(e => e.TotalAmount, e => e.ToMoneyInput().Currency("USD"))
            .ToDialog(isOpen, title: "Create Order", submitTitle: "Create");
    }

    private long CreateOrder(NorthwindContextFactory factory, OrderCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var order = new Order
        {
            CustomerId = customerId,
            OrderDate = request.OrderDate,
            ShippedDate = request.ShippedDate,
            TotalAmount = request.TotalAmount
        };

        db.Orders.Add(order);
        db.SaveChanges();

        return order.OrderId;
    }
}