namespace Northwind.Apps.Views;

public class OrderItemEditSheet(IState<bool> isOpen, RefreshToken refreshToken, long orderItemId) : ViewBase
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
            .Builder(e => e.Quantity, e => e.ToFeedbackInput())
            .Remove(e => e.OrderItemId)
            .Builder(e => e.OrderId, e => e.ToAsyncSelectInput(QueryOrders(factory), LookupOrder(factory), placeholder: "Select Order"))
            .Builder(e => e.ProductId, e => e.ToAsyncSelectInput(QueryProducts(factory), LookupProduct(factory), placeholder: "Select Product"))
            .ToSheet(isOpen, "Edit Order Item");
    }

    private static AsyncSelectQueryDelegate<long?> QueryOrders(NorthwindContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Orders
                    .Where(e => e.OrderId.ToString().Contains(query))
                    .Select(e => new { e.OrderId, e.OrderDate })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<long?>(e.OrderDate.ToString("yyyy-MM-dd"), e.OrderId))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<long?> LookupOrder(NorthwindContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var order = await db.Orders.FirstOrDefaultAsync(e => e.OrderId == id);
            if (order == null) return null;
            return new Option<long?>(order.OrderDate.ToString("yyyy-MM-dd"), order.OrderId);
        };
    }

    private static AsyncSelectQueryDelegate<long?> QueryProducts(NorthwindContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Products
                    .Where(e => e.ProductName.Contains(query))
                    .Select(e => new { e.ProductId, e.ProductName })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<long?>(e.ProductName, e.ProductId))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<long?> LookupProduct(NorthwindContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var product = await db.Products.FirstOrDefaultAsync(e => e.ProductId == id);
            if (product == null) return null;
            return new Option<long?>(product.ProductName, product.ProductId);
        };
    }
}