using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Northwind.Connections.Northwind;

[Table("order_items", Schema = "nw3")]
public partial class OrderItem
{
    [Key]
    [Column("order_item_id")]
    public long OrderItemId { get; set; }

    [Column("order_id")]
    public long? OrderId { get; set; }

    [Column("product_id")]
    public long? ProductId { get; set; }

    [Column("quantity")]
    public long Quantity { get; set; }

    [Column("unit_price")]
    public double UnitPrice { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderItems")]
    public virtual Order? Order { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("OrderItems")]
    public virtual Product? Product { get; set; }
}
