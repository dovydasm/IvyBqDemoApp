namespace Northwind.Apps.Views;

public class OrderCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record OrderCreateRequest
    {
        [Required]
        public long? CustomerId { get; init; } = null;

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
            .Builder(e => e.CustomerId, e => e.ToAsyncSelectInput(QueryCustomers(factory), LookupCustomer(factory), placeholder: "Select Customer"))
            .Builder(e => e.TotalAmount, e => e.ToMoneyInput().Currency("USD"))
            .ToDialog(isOpen, title: "Create Order", submitTitle: "Create");
    }

    private long CreateOrder(NorthwindContextFactory factory, OrderCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var order = new Order
        {
            CustomerId = request.CustomerId,
            OrderDate = request.OrderDate,
            ShippedDate = request.ShippedDate,
            TotalAmount = request.TotalAmount
        };

        db.Orders.Add(order);
        db.SaveChanges();

        return order.OrderId;
    }

    private static AsyncSelectQueryDelegate<long?> QueryCustomers(NorthwindContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Customers
                    .Where(c => c.CompanyName.Contains(query))
                    .Select(c => new { c.CustomerId, c.CompanyName })
                    .Take(50)
                    .ToArrayAsync())
                .Select(c => new Option<long?>(c.CompanyName, c.CustomerId))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<long?> LookupCustomer(NorthwindContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null) return null;
            return new Option<long?>(customer.CompanyName, customer.CustomerId);
        };
    }
}