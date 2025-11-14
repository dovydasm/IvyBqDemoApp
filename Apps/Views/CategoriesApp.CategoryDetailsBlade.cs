namespace Northwind.Apps.Views;

public class CategoryDetailsBlade(long categoryId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<NorthwindContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var category = this.UseState<Category?>();
        var productCount = this.UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            var db = factory.CreateDbContext();
            category.Set(await db.Categories.SingleOrDefaultAsync(e => e.CategoryId == categoryId));
            productCount.Set(await db.Products.CountAsync(e => e.CategoryId == categoryId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (category.Value == null) return null;

        var categoryValue = category.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this category?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Category", AlertButtonSet.OkCancel);
        };

        var dropDown = Icons.Ellipsis
            .ToButton()
            .Ghost()
            .WithDropDown(
                MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(OnDelete)
            );

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .ToTrigger((isOpen) => new CategoryEditSheet(isOpen, refreshToken, categoryId));

        var detailsCard = new Card(
            content: new
                {
                    categoryValue.CategoryId,
                    categoryValue.CategoryName,
                    categoryValue.Description
                }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.CategoryId, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Category Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Products", onClick: _ =>
                {
                    blades.Push(this, new CategoryProductsBlade(categoryId), "Products");
                }, badge: productCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(NorthwindContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var category = db.Categories.FirstOrDefault(e => e.CategoryId == categoryId)!;
        db.Categories.Remove(category);
        db.SaveChanges();
    }
}