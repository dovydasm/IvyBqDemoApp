namespace Northwind.Apps.Views;

public class CategoryProductsBlade(long? categoryId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var refreshToken = this.UseRefreshToken();
        var products = this.UseState<Product[]?>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            if (categoryId == null) return;

            await using var db = factory.CreateDbContext();
            products.Set(await db.Products
                .Where(p => p.CategoryId == categoryId)
                .ToArrayAsync());
        }, [EffectTrigger.AfterInit(), refreshToken]);

        Action OnDelete(long productId)
        {
            return () =>
            {
                showAlert("Are you sure you want to delete this product?", result =>
                {
                    if (result.IsOk())
                    {
                        Delete(factory, productId);
                        refreshToken.Refresh();
                    }
                }, "Delete Product", AlertButtonSet.OkCancel);
            };
        }

        if (products.Value == null) return null;

        var table = products.Value.Select(p => new
            {
                ProductName = p.ProductName,
                UnitPrice = p.UnitPrice,
                UnitsInStock = p.UnitsInStock,
                _ = Layout.Horizontal().Gap(1)
                    | Icons.Ellipsis
                        .ToButton()
                        .Ghost()
                        .WithDropDown(MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete(p.ProductId)))
                    | Icons.Pencil
                        .ToButton()
                        .Outline()
                        .Tooltip("Edit")
                        .ToTrigger((isOpen) => new CategoryProductsEditSheet(isOpen, refreshToken, p.ProductId))
            })
            .ToTable()
            .RemoveEmptyColumns();

        var addBtn = new Button("Add Product").Icon(Icons.Plus).Outline()
            .ToTrigger((isOpen) => new CategoryProductsCreateDialog(isOpen, refreshToken, categoryId));

        return new Fragment()
               | BladeHelper.WithHeader(addBtn, table)
               | alertView;
    }

    public void Delete(NorthwindContextFactory factory, long productId)
    {
        using var db = factory.CreateDbContext();
        var product = db.Products.Single(p => p.ProductId == productId);
        db.Products.Remove(product);
        db.SaveChanges();
    }
}