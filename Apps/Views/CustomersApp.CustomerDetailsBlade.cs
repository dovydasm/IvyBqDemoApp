namespace Northwind.Apps.Views;

public class CustomerDetailsBlade(long customerId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<NorthwindContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var customer = this.UseState<Customer?>();
        var orderCount = this.UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            using var db = factory.CreateDbContext();
            customer.Set(await db.Customers.SingleOrDefaultAsync(c => c.CustomerId == customerId));
            orderCount.Set(await db.Orders.CountAsync(o => o.CustomerId == customerId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (customer.Value == null) return null;

        var customerValue = customer.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this customer?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Customer", AlertButtonSet.OkCancel);
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
            .ToTrigger((isOpen) => new CustomerEditSheet(isOpen, refreshToken, customerId));

        var detailsCard = new Card(
            content: new
                {
                    customerValue.CustomerId,
                    customerValue.CompanyName,
                    customerValue.ContactName,
                    customerValue.Email,
                    customerValue.Country
                }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.CustomerId, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Customer Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Orders", onClick: _ =>
                {
                    blades.Push(this, new CustomerOrdersBlade(customerId), "Orders");
                }, badge: orderCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(NorthwindContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var customer = db.Customers.FirstOrDefault(c => c.CustomerId == customerId)!;
        db.Customers.Remove(customer);
        db.SaveChanges();
    }
}