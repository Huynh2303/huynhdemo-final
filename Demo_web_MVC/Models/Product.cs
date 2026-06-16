using System;
using System.Collections.Generic;

namespace Demo_web_MVC.Models;

public partial class Product
{
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public int? SellerId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Brand { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public bool IsDeleted { get; set; } = false;
    public virtual User? Seller { get; set; } 
}
