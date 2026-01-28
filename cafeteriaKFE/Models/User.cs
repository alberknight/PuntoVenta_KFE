using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cafeteriaKFE.Models;

[Index("RoleId", "Deleted", Name = "IX_Users_RoleId_Deleted")]
[Index("Email", Name = "UQ_Users_Email", IsUnique = true)]
public partial class User
{
    [Key]
    public long UserId { get; set; }

    [StringLength(80)]
    public string Name { get; set; } = null!;

    [StringLength(80)]
    public string Lastname { get; set; } = null!;

    [StringLength(120)]
    public string Email { get; set; } = null!;

    [StringLength(30)]
    public string? Phone { get; set; }

    public int RoleId { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    public bool Deleted { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual Role Role { get; set; } = null!;
}
