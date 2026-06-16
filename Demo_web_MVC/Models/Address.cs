using System;
using System.Collections.Generic;

namespace Demo_web_MVC.Models;

public partial class Address
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string AddressLine { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Country { get; set; } = null!;

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }
    public string RecipientName { get; set; } = null!; 
    public string PhoneNumber { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
