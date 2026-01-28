using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cafeteriaKFE.Models;

[Index("Name", Name = "UQ_PaidMethods_Name", IsUnique = true)]
public partial class PaidMethod
{
    [Key]
    public int PaidMethodId { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    public bool Deleted { get; set; }

    [InverseProperty("PaidMethod")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
