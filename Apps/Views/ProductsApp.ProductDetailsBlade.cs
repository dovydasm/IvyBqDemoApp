namespace Northwind.Apps.Views;

public class ProductDetailsBlade(long productId) : ViewBase
{
    public override object? Build()
    {
        var factory = this.UseService<NorthwindContextFactory>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var product = this.UseState<Product?>();
        var orderItemCount = this.UseState<int>();
        var (alertView, showAlert) = this.UseAlert();

        this.UseEffect(async () =>
        {
            using var db = factory.CreateDbContext();
            product.Set(await db.Products.Include(p => p.Category).SingleOrDefaultAsync(p => p.ProductId == productId));
            orderItemCount.Set(await db.OrderItems.CountAsync(oi => oi.ProductId == productId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (product.Value == null) return null;

        var productValue = product.Value;

        void OnDelete()
        {
            showAlert("Are you sure you want to delete this product?", result =>
            {
                if (result.IsOk())
                {
                    Delete(factory);
                    blades.Pop(refresh: true);
                }
            }, "Delete Product", AlertButtonSet.OkCancel);
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
            .ToTrigger((isOpen) => new ProductEditSheet(isOpen, refreshToken, productId));

        var detailsCard = new Card(
            content: new
                {
                    productValue.ProductId,
                    productValue.ProductName,
                    CategoryName = productValue.Category?.CategoryName,
                    productValue.UnitPrice,
                    productValue.UnitsInStock
                }.ToDetails()
                .RemoveEmpty()
                .Builder(e => e.ProductId, e => e.CopyToClipboard()),
            footer: Layout.Horizontal().Width(Size.Full()).Gap(1).Align(Align.Right)
                    | dropDown
                    | editBtn
        ).Title("Product Details");

        var relatedCard = new Card(
            new List(
                new ListItem("Order Items", onClick: _ =>
                {
                    blades.Push(this, new ProductOrderItemsBlade(productId), "Order Items");
                }, badge: orderItemCount.Value.ToString("N0"))
            ));

        return new Fragment()
               | (Layout.Vertical() | detailsCard | relatedCard)
               | alertView;
    }

    private void Delete(NorthwindContextFactory dbFactory)
    {
        using var db = dbFactory.CreateDbContext();
        var product = db.Products.FirstOrDefault(p => p.ProductId == productId)!;
        db.Products.Remove(product);
        db.SaveChanges();
    }
}