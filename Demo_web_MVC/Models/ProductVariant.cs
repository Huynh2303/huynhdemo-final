using System;
using System.Collections.Generic;

namespace Demo_web_MVC.Models;

public partial class ProductVariant
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string? Size { get; set; }

    public string? Color { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Product Product { get; set; } = null!;
    
    public ICollection<ProductVariantImage> ProductVariantImages { get; set; } = new List<ProductVariantImage>();
}
