namespace Northwind.Apps.Views;

public class CustomerEditSheet(IState<bool> isOpen, RefreshToken refreshToken, long customerId) : ViewBase
{
    public override object? Build()
    {
        var factory = UseService<NorthwindContextFactory>();
        var customer = UseState(() => factory.CreateDbContext().Customers.FirstOrDefault(e => e.CustomerId == customerId)!);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            try
            {
                using var db = factory.CreateDbContext();
                db.Customers.Update(customer.Value);
                db.SaveChanges();
                refreshToken.Refresh();
            }
            catch (Exception ex)
            {
                client.Toast(ex);
            }
        }, [customer]);

        return customer
            .ToForm()
            .Builder(e => e.Email, e => e.ToEmailInput())
            .Builder(e => e.Country, e => e.ToTextAreaInput())
            .Place(e => e.CompanyName, e => e.ContactName)
            .Group("Contact Information", e => e.Email, e => e.Country)
            .Remove(e => e.CustomerId)
            .ToSheet(isOpen, "Edit Customer");
    }
}