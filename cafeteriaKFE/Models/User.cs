using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; // Added for IdentityUser

namespace cafeteriaKFE.Models;

public partial class User : IdentityUser<long> // Inherit from IdentityUser<long>
{
    // Removed: UserId as IdentityUser provides Id.
    // Removed: Email as IdentityUser provides Email.

    [StringLength(80)]
    public string Name { get; set; } = null!;

    [StringLength(80)]
    public string LastName { get; set; } = null!;

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime UpdatedAt { get; set; }

    public bool Deleted { get; set; }
}
