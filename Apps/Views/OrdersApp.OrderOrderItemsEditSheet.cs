namespace Northwind.Apps.Views;

public class OrderOrderItemsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, long orderItemId) : ViewBase
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
            .Builder(e => e.ProductId, e => e.ToAsyncSelectInput(QueryProducts(factory), LookupProduct(factory), placeholder: "Select Product"))
            .Builder(e => e.UnitPrice, e => e.ToMoneyInput().Currency("USD"))
            .Place(e => e.Quantity, e => e.UnitPrice)
            .Remove(e => e.OrderId, e => e.OrderItemId)
            .ToSheet(isOpen, "Edit Order Item");
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