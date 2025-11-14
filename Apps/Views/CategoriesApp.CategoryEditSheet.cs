namespace Northwind.Apps.Views;

public class CategoryEditSheet(IState<bool> isOpen, RefreshToken refreshToken, long categoryId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var category = UseState(() => factory.CreateDbContext().Categories.FirstOrDefault(e => e.CategoryId == categoryId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                db.Categories.Update(category.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [category]);

        return category
            .ToForm()
            .Builder(e => e.Description, e => e.ToTextAreaInput())
            .Place(e => e.CategoryName, e => e.Description)
            .Remove(e => e.CategoryId)
            .ToSheet(isOpen, "Edit Category");
    }
}