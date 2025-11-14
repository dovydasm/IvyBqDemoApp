namespace Northwind.Apps.Views;

public class CustomerCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    private record CustomerCreateRequest
    {
        [Required]
        public string CompanyName { get; init; } = "";

        public string? ContactName { get; init; }

        [EmailAddress]
        public string? Email { get; init; }

        public string? Country { get; init; }
    }

    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var customer = UseState(() => new CustomerCreateRequest());
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                var customerId = CreateCustomer(factory, customer.Value);
                refreshToken.Refresh(customerId);
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [customer]);

        return customer
            .ToForm()
            .ToDialog(isOpen, title: "Create Customer", submitTitle: "Create");
    }

    private long CreateCustomer(NorthwindContextFactory factory, CustomerCreateRequest request)
    {
        using var db = factory.CreateDbContext();

        var customer = new Customer()
        {
            CompanyName = request.CompanyName,
            ContactName = request.ContactName,
            Email = request.Email,
            Country = request.Country
        };

        db.Customers.Add(customer);
        db.SaveChanges();

        return customer.CustomerId;
    }
}