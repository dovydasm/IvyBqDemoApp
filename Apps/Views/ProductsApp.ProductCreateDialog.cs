namespace Northwind.Apps.Views;

public class ProductCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record ProductCreateRequest
    {
        [Required]
        public string ProductName { get; init; } = "";

        [Required]
        public long? CategoryId { get; init; } = null;

        [Required]
        public double UnitPrice { get; init; }

        public long? UnitsInStock { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var productState = UseState(() => new ProductCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                var productId = CreateProduct(factory, productState.Value);
                refreshToken.Refresh(productId);
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [productState]);

        return productState
            .ToForm()
            .Builder(e => e.CategoryId, e => e.ToAsyncSelectInput(QueryCategories(factory), LookupCategory(factory), placeholder: "Select Category"))
            .Builder(e => e.UnitPrice, e => e.ToMoneyInput().Currency("USD"))
            .ToDialog(isOpen, title: "Create Product", submitTitle: "Create");
    }

    private long CreateProduct(NorthwindContextFactory factory, ProductCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var product = new Product
        {
            ProductName = request.ProductName,
            CategoryId = request.CategoryId,
            UnitPrice = request.UnitPrice,
            UnitsInStock = request.UnitsInStock ?? 0
        };

        db.Products.Add(product);
        db.SaveChanges();

        return product.ProductId;
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