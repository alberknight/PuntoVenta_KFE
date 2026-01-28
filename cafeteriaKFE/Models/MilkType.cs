using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cafeteriaKFE.Models;

[Index("Name", Name = "UQ_MilkTypes_Name", IsUnique = true)]
public partial class MilkType
{
    [Key]
    public int MilkTypeId { get; set; }

    [StringLength(60)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal PriceDelta { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    public bool Deleted { get; set; }

    [InverseProperty("MilkType")]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
