namespace Northwind.Apps.Views;

public class CategoryProductsEditSheet(IState<bool> isOpen, RefreshToken refreshToken, long productId) : ViewBase
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
            .Builder(e => e.UnitsInStock, e => e.ToFeedbackInput())
            .Remove(e => e.ProductId, e => e.CategoryId)
            .ToSheet(isOpen, "Edit Product");
    }
}