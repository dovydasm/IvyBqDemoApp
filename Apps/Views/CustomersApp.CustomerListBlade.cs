namespace Northwind.Apps.Views;

public class CustomerListBlade : ViewBase
{
    private record CustomerListRecord(long Id, string CompanyName, string? ContactName, string? Email, string? Country);

    public override object? Build()
    {
        var blades = UseContext<IBladeController>();
        var factory = UseService<NorthwindContextFactory>();
        var refreshToken = this.UseRefreshToken();

        UseEffect(() =>
        {
            if (refreshToken.ReturnValue is long customerId)
            {
                blades.Pop(this, true);
                blades.Push(this, new CustomerDetailsBlade(customerId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var customer = (CustomerListRecord)e.Sender.Tag!;
            blades.Push(this, new CustomerDetailsBlade(customer.Id), customer.CompanyName);
        });

        ListItem CreateItem(CustomerListRecord record) =>
            new(title: record.CompanyName, subtitle: record.ContactName, onClick: onItemClicked, tag: record);

        var createBtn = Icons.Plus.ToButton(_ =>
        {
            blades.Pop(this);
        }).Outline().Tooltip("Create Customer").ToTrigger((isOpen) => new CustomerCreateDialog(isOpen, refreshToken));

        return new FilteredListView<CustomerListRecord>(
            fetchRecords: (filter) => FetchCustomers(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<CustomerListRecord[]> FetchCustomers(NorthwindContextFactory factory, string filter)
    {
        await using var db = factory.CreateDbContext();

        var query = db.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = filter.Trim();
            query = query.Where(c => c.CompanyName.Contains(filter) || c.ContactName.Contains(filter) || c.Email.Contains(filter) || c.Country.Contains(filter));
        }

        return await query
            .OrderBy(c => c.CompanyName)
            .Take(50)
            .Select(c => new CustomerListRecord(c.CustomerId, c.CompanyName, c.ContactName, c.Email, c.Country))
            .ToArrayAsync();
    }
}