namespace Northwind.Apps.Views;

public class CategoryProductsCreateDialog(IState<bool> isOpen, RefreshToken refreshToken, long? categoryId) : ViewBase
{
    private record ProductCreateRequest
    {
        [Required]
        public string ProductName { get; init; } = "";

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
            .Builder(e => e.UnitPrice, e => e.ToMoneyInput().Currency("USD"))
            .ToDialog(isOpen, title: "Create Product", submitTitle: "Create");
    }

    private long CreateProduct(NorthwindContextFactory factory, ProductCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var product = new Product
        {
            ProductName = request.ProductName,
            UnitPrice = request.UnitPrice,
            UnitsInStock = request.UnitsInStock,
            CategoryId = categoryId
        };

        db.Products.Add(product);
        db.SaveChanges();

        return product.ProductId;
    }
}