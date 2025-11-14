namespace Northwind.Apps.Views;

public class ProductListBlade : ViewBase
{
    private record ProductListRecord(long Id, string Name, string? CategoryName);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<NorthwindContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is long productId)
            {
                blades.Pop(this, true);
                blades.Push(this, new ProductDetailsBlade(productId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var product = (ProductListRecord)e.Sender.Tag!;
            blades.Push(this, new ProductDetailsBlade(product.Id), product.Name);
        });

        ListItem CreateItem(ProductListRecord record) =>
            new(title: record.Name, subtitle: record.CategoryName, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Product").ToTrigger(isOpen => new ProductCreateDialog(isOpen, refreshToken));

        return new FilteredListView<ProductListRecord>(
            fetchRecords: filter => FetchProducts(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<ProductListRecord[]> FetchProducts(NorthwindContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(e => e.ProductName.Contains(filter) || (e.Category != null && e.Category.CategoryName.Contains(filter)));
        }

        return await linq
            .OrderBy(e => e.ProductName)
            .Take(50)
            .Select(e => new ProductListRecord(e.ProductId, e.ProductName, e.Category != null ? e.Category.CategoryName : null))
            .ToArrayAsync();
    }
}