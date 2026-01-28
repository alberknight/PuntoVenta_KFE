using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cafeteriaKFE.Models;

[Index("Label", Name = "UQ_Temperatures_Label", IsUnique = true)]
public partial class Temperature
{
    [Key]
    public int TemperatureId { get; set; }

    [StringLength(20)]
    public string Label { get; set; } = null!;

    public int DegreesC { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal PriceDelta { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    public bool Deleted { get; set; }

    [InverseProperty("Temperature")]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
