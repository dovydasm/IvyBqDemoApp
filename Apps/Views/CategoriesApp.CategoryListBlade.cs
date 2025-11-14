namespace Northwind.Apps.Views;

public class CategoryListBlade : ViewBase
{
    private record CategoryListRecord(long Id, string Name, string? Description);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<NorthwindContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is long categoryId)
            {
                blades.Pop(this, true);
                blades.Push(this, new CategoryDetailsBlade(categoryId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var category = (CategoryListRecord)e.Sender.Tag!;
            blades.Push(this, new CategoryDetailsBlade(category.Id), category.Name);
        });

        ListItem CreateItem(CategoryListRecord record) =>
            new(title: record.Name, subtitle: record.Description, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Category").ToTrigger((isOpen) => new CategoryCreateDialog(isOpen, refreshToken));

        return new FilteredListView<CategoryListRecord>(
            fetchRecords: (filter) => FetchCategories(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<CategoryListRecord[]> FetchCategories(NorthwindContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.CategoryName.Contains(filter) || (e.Description != null && e.Description.Contains(filter)));
        }

        return await linq
            .OrderBy(e => e.CategoryName)
            .Take(50)
            .Select(e => new CategoryListRecord(e.CategoryId, e.CategoryName, e.Description))
            .ToArrayAsync();
    }
}