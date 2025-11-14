namespace Northwind.Apps.Views;

public class ProductEditSheet(IState<bool> isOpen, RefreshToken refreshToken, long productId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var product = UseState(() => factory.CreateDbContext().Products.FirstOrDefault(e => e.ProductId == productId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                db.Products.Update(product.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [product]);

        return product
            .ToForm()
            .Builder(e => e.ProductName, e => e.ToTextAreaInput())
            .Builder(e => e.UnitPrice, e => e.ToMoneyInput().Currency("USD"))
            .Builder(e => e.CategoryId, e => e.ToAsyncSelectInput(QueryCategories(factory), LookupCategory(factory), placeholder: "Select Category"))
            .Place(e => e.ProductName, e => e.UnitPrice, e => e.CategoryId)
            .Remove(e => e.ProductId, e => e.OrderItems)
            .ToSheet(isOpen, "Edit Product");
    }

    private static AsyncSelectQueryDelegate<long?> QueryCategories(NorthwindContextFactory factory)
    {
        return async query =>
        {
            await using var db = factory.CreateDbContext();
            return (await db.Categories
                    .Where(e => e.CategoryName.Contains(query))
                    .Select(e => new { e.CategoryId, e.CategoryName })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<long?>(e.CategoryName, e.CategoryId))
                .ToArray();
        };
    }

    private static AsyncSelectLookupDelegate<long?> LookupCategory(NorthwindContextFactory factory)
    {
        return async id =>
        {
            if (id == null) return null;
            await using var db = factory.CreateDbContext();
            var category = await db.Categories.FirstOrDefaultAsync(e => e.CategoryId == id);
            if (category == null) return null;
            return new Option<long?>(category.CategoryName, category.CategoryId);
        };
    }
}