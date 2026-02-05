using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cafeteriaKFE.Models;

[Table("OrderDetail")]
[Index("OrderId", "Deleted", Name = "IX_OrderDetail_OrderId_Deleted")]
public partial class OrderDetail
{
    [Key]
    public long OrderDetailId { get; set; }

    public long OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public int? SizeId { get; set; }

    public int? MilkTypeId { get; set; }

    public bool HasWhippedCream { get; set; }

    public int? SyrupId { get; set; }

    public int? TemperatureId { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    public bool Deleted { get; set; }

    [ForeignKey("MilkTypeId")]
    [InverseProperty("OrderDetails")]
    public virtual MilkType MilkType { get; set; } = null!;

    [ForeignKey("OrderId")]
    [InverseProperty("OrderDetails")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("OrderDetails")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("SizeId")]
    [InverseProperty("OrderDetails")]
    public virtual Size Size { get; set; } = null!;

    [ForeignKey("SyrupId")]
    [InverseProperty("OrderDetails")]
    public virtual Syrup? Syrup { get; set; }

    [ForeignKey("TemperatureId")]
    [InverseProperty("OrderDetails")]
    public virtual Temperature? Temperature { get; set; }
}
