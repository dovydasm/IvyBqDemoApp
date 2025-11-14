namespace Northwind.Apps.Views;

public class OrderEditSheet(IState<bool> isOpen, RefreshToken refreshToken, long orderId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var order = UseState(() => factory.CreateDbContext().Orders.FirstOrDefault(e => e.OrderId == orderId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                db.Orders.Update(order.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [order]);

        return order
            .ToForm()
            .Builder(e => e.TotalAmount, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.CustomerId, e => e.ToAsyncSelectInput(QueryCustomers(factory), LookupCustomer(factory), placeholder: "Select Customer"))
            .Place(e => e.OrderDate, e => e.ShippedDate)
            .Remove(e => e.OrderId)
            .ToSheet(isOpen, "Edit Order");
    }

    private static AsyncSelectQueryDelegate<long?> QueryCustomers(NorthwindContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Customers
                    .Where(e => e.CompanyName.Contains(query))
                    .Select(e => new { e.CustomerId, e.CompanyName })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<long?>(e.CompanyName, e.CustomerId))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<long?> LookupCustomer(NorthwindContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var customer = await db.Customers.FirstOrDefaultAsync(e => e.CustomerId == id);
            if (customer == null) return null;
            return new Option<long?>(customer.CompanyName, customer.CustomerId);
        };
    }
}