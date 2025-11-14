namespace Northwind.Apps.Views;

public class OrderOrderItemsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, long? orderId) : ViewBase
{
    private record OrderItemCreateRequest
    {
        [Required]
        public long? ProductId { get; init; } = null;

        [Required]
        public long Quantity { get; init; }

        [Required]
        public double UnitPrice { get; init; }
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
            .Builder(e => e.ProductId, e => e.ToAsyncSelectInput(QueryProducts(factory), LookupProduct(factory), placeholder: "Select Product"))
            .Builder(e => e.UnitPrice, e => e.ToMoneyInput().Currency("USD"))
            .ToDialog(isOpen, title: "Create Order Item", submitTitle: "Create");
    }

    private long CreateOrderItem(NorthwindContextFactory factory, OrderItemCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var orderItem = new OrderItem
        {
            OrderId = orderId,
            ProductId = request.ProductId!.Value,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice
        };

        db.OrderItems.Add(orderItem);
        db.SaveChanges();

        return orderItem.OrderItemId;
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