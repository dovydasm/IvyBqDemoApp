namespace Northwind.Apps.Views;

public class OrderItemCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record OrderItemCreateRequest
    {
        [Required]
        public long? OrderId { get; init; } = null;

        [Required]
        public long? ProductId { get; init; } = null;

        [Required]
        public long Quantity { get; init; } = 1;

        [Required]
        public double UnitPrice { get; init; } = 0.0;
    }

    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var orderItem = UseState(() => new OrderItemCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                var orderItemId = CreateOrderItem(factory, orderItem.Value);
                refreshToken.Refresh(orderItemId);
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [orderItem]);

        return orderItem
            .ToForm()
            .Builder(e => e.OrderId, e => e.ToAsyncSelectInput(QueryOrders(factory), LookupOrder(factory), placeholder: "Select Order"))
            .Builder(e => e.ProductId, e => e.ToAsyncSelectInput(QueryProducts(factory), LookupProduct(factory), placeholder: "Select Product"))
            .Builder(e => e.UnitPrice, e => e.ToMoneyInput().Currency("USD"))
            .ToDialog(isOpen, title: "Create Order Item", submitTitle: "Create");
    }

    private long CreateOrderItem(NorthwindContextFactory factory, OrderItemCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var orderItem = new OrderItem()
        {
            OrderId = request.OrderId!.Value,
            ProductId = request.ProductId!.Value,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice
        };

        db.OrderItems.Add(orderItem);
        db.SaveChanges();

        return orderItem.OrderItemId;
    }

    private static AsyncSelectQueryDelegate<long?> QueryOrders(NorthwindContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Orders
                    .Where(o => o.OrderId.ToString().Contains(query))
                    .Select(o => new { o.OrderId, o.OrderDate })
                    .Take(50)
                    .ToArrayAsync())
                .Select(o => new Option<long?>($"Order {o.OrderId} - {o.OrderDate:yyyy-MM-dd}", o.OrderId))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<long?> LookupOrder(NorthwindContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return null;
            return new Option<long?>($"Order {order.OrderId} - {order.OrderDate:yyyy-MM-dd}", order.OrderId);
        };
    }

    private static AsyncSelectQueryDelegate<long?> QueryProducts(NorthwindContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Products
                    .Where(p => p.ProductName.Contains(query))
                    .Select(p => new { p.ProductId, p.ProductName })
                    .Take(50)
                    .ToArrayAsync())
                .Select(p => new Option<long?>(p.ProductName, p.ProductId))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<long?> LookupProduct(NorthwindContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var product = await db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null) return null;
            return new Option<long?>(product.ProductName, product.ProductId);
        };
    }
}