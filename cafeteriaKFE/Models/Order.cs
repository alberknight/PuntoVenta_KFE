using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cafeteriaKFE.Models;

[Index("CreatedAt", "Deleted", Name = "IX_Orders_CreatedAt_Deleted")]
public partial class Order
{
    [Key]
    public long OrderId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal TotalAmount { get; set; }

    public bool IsDelivery { get; set; }

    public int PaidMethodId { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    public bool Deleted { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    [ForeignKey("PaidMethodId")]
    [InverseProperty("Orders")]
    public virtual PaidMethod PaidMethod { get; set; } = null!;
}
