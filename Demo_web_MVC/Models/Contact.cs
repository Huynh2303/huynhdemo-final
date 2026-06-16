using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Demo_web_MVC.Models;

public partial class Contact
{
    public int id { get; set; }
    public string? name { get; set; }
    [EmailAddress]
    public string? email { get; set; }
    [Phone]
    public string? phone { get; set; }
    [DataType(DataType.Date)]
    public DateTime? DateSend { get; set; }
    [Required(ErrorMessage = "Please enter a message")]
    public string? Message { get; set; }
}
