using System;
using System.Collections.Generic;

namespace Demo_web_MVC.Models;

public partial class Category
{
    public int? Id { get; set; }

    public string? Name { get; set; } = null!;

    public  int? ParentId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Category> InverseParent { get; set; } = new List<Category>();

    public virtual Category? Parent { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
