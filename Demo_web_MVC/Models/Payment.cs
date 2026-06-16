using System;
using System.Collections.Generic;

namespace Demo_web_MVC.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public  PaymentMethod Method { get; set; } 

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
