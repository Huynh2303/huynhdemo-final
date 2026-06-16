using System;
using System.Collections.Generic;

namespace Demo_web_MVC.Models;

public partial class UserToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; } = null!;

    public TokenType Type { get; set; }

    public DateTime ExpiredAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTime? UsedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
public enum TokenType
{
    EmailConfirm = 1,
    ResetPassword = 2
}