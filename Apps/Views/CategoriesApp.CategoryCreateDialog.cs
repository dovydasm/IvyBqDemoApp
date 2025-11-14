namespace Northwind.Apps.Views;

public class CategoryCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record CategoryCreateRequest
    {
        [Required]
        public string CategoryName { get; init; } = "";

        public string? Description { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var categoryState = UseState(() => new CategoryCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                var categoryId = CreateCategory(factory, categoryState.Value);
                refreshToken.Refresh(categoryId);
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [categoryState]);

        return categoryState
            .ToForm()
            .ToDialog(isOpen, title: "Create Category", submitTitle: "Create");
    }

    private long CreateCategory(NorthwindContextFactory factory, CategoryCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var category = new Category
        {
            CategoryName = request.CategoryName,
            Description = request.Description
        };

        db.Categories.Add(category);
        db.SaveChanges();

        return category.CategoryId;
    }
}