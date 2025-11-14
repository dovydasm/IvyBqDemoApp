using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Northwind.Connections.Northwind;

[Table("orders", Schema = "nw3")]
public partial class Order
{
    [Key]
    [Column("order_id")]
    public long OrderId { get; set; }

    [Column("customer_id")]
    public long? CustomerId { get; set; }

    [Column("order_date")]
    public DateOnly OrderDate { get; set; }

    [Column("shipped_date")]
    public DateOnly? ShippedDate { get; set; }

    [Column("total_amount")]
    public double? TotalAmount { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("Orders")]
    public virtual Customer? Customer { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
