using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cafeteriaKFE.Models;

[Index("Name", Name = "UQ_ProductTypes_Name", IsUnique = true)]
public partial class ProductType
{
    [Key]
    public int ProductTypeId { get; set; }

    [StringLength(60)]
    public string Name { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    public bool Deleted { get; set; }

    [InverseProperty("ProductType")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
