namespace Northwind.Apps.Views;

public class OrderListBlade : ViewBase
{
    private record OrderListRecord(long Id, string CustomerName, DateOnly OrderDate, double? TotalAmount);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<NorthwindContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is long orderId)
            {
                blades.Pop(this, true);
                blades.Push(this, new OrderDetailsBlade(orderId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var order = (OrderListRecord)e.Sender.Tag!;
            blades.Push(this, new OrderDetailsBlade(order.Id), $"Order #{order.Id}");
        });

        ListItem CreateItem(OrderListRecord record) =>
            new(title: $"Order #{record.Id}", subtitle: $"{record.CustomerName} - {record.OrderDate:yyyy-MM-dd} - ${record.TotalAmount:F2}", onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Order").ToTrigger((isOpen) => new OrderCreateDialog(isOpen, refreshToken));

        return new FilteredListView<OrderListRecord>(
            fetchRecords: (filter) => FetchOrders(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<OrderListRecord[]> FetchOrders(NorthwindContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var linq = db.Orders.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            linq = linq.Where(o => o.Customer != null && (o.Customer.CompanyName.Contains(filter) || o.Customer.ContactName.Contains(filter)));
        }

        return await linq
            .OrderByDescending(o => o.OrderDate)
            .Take(50)
            .Select(o => new OrderListRecord(o.OrderId, o.Customer != null ? o.Customer.CompanyName : "Unknown", o.OrderDate, o.TotalAmount))
            .ToArrayAsync();
    }
}