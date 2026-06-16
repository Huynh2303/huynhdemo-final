using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Demo_web_MVC.Models;

public partial class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên đăng nhập chỉ được chứa chữ, số và dấu gạch dưới")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
    public string? FullName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? EmailConfirmedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    [Range(0, 10, ErrorMessage = "Số lần đăng nhập sai không hợp lệ")]
    public int FailedLoginCount { get; set; } = 0;

    public DateTime? LockoutUntil { get; set; }

    public bool IsVip { get; set; } = false;

    [DataType(DataType.Date)]
    [CustomValidation(typeof(User), nameof(ValidateDateOfBirth))]
    public DateTime? DateOfBirth { get; set; }

    public int? LastBirthdayEmailYear { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public UserImage? UserImage { get; set; }

    public virtual ICollection<Product> SellerProducts { get; set; } = new List<Product>();

    public static ValidationResult? ValidateDateOfBirth(DateTime? dateOfBirth, ValidationContext context)
    {
        if (dateOfBirth == null)
        {
            return ValidationResult.Success;
        }

        if (dateOfBirth.Value.Date > DateTime.Now.Date)
        {
            return new ValidationResult("Ngày sinh không được lớn hơn ngày hiện tại");
        }

        if (dateOfBirth.Value.Date < DateTime.Now.AddYears(-120).Date)
        {
            return new ValidationResult("Ngày sinh không hợp lệ");
        }

        return ValidationResult.Success;
    }
}