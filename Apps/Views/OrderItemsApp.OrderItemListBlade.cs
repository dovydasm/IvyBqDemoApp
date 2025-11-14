namespace Northwind.Apps.Views;

public class OrderItemListBlade : ViewBase
{
    private record OrderItemListRecord(long Id, string ProductName, long Quantity, double UnitPrice);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<NorthwindContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is long orderItemId)
            {
                blades.Pop(this, true);
                blades.Push(this, new OrderItemDetailsBlade(orderItemId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var orderItem = (OrderItemListRecord)e.Sender.Tag!;
            blades.Push(this, new OrderItemDetailsBlade(orderItem.Id), orderItem.ProductName);
        });

        ListItem CreateItem(OrderItemListRecord record) =>
            new(title: record.ProductName, subtitle: $"Quantity: {record.Quantity}, Unit Price: {record.UnitPrice:C}", onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Order Item").ToTrigger((isOpen) => new OrderItemCreateDialog(isOpen, refreshToken));

        return new FilteredListView<OrderItemListRecord>(
            fetchRecords: (filter) => FetchOrderItems(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<OrderItemListRecord[]> FetchOrderItems(NorthwindContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.OrderItems.Include(o => o.Product).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(o => o.Product != null && o.Product.ProductName.Contains(filter));
        }

        return await linq
            .OrderByDescending(o => o.OrderItemId)
            .Take(50)
            .Select(o => new OrderItemListRecord(
                o.OrderItemId,
                o.Product != null ? o.Product.ProductName : "Unknown Product",
                o.Quantity,
                o.UnitPrice))
            .ToArrayAsync();
    }
}