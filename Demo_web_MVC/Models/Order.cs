using System;
using System.Collections.Generic;

namespace Demo_web_MVC.Models;

public partial class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal TotalAmount { get; set; }

    //public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public PaymentMethod PaymentMethod { get; set; } // COD / MoMo

    public virtual FraudAnalysis? FraudAnalysis { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<OrderLog> OrderLogs { get; set; } = new List<OrderLog>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User User { get; set; } = null!;
    public OrderStatus Status { get; set; }
    public bool CanCompleteDelivery()
    {
        return Status == OrderStatus.Shipping;
    }

    public void CompleteDelivery()
    {
        if (!CanCompleteDelivery())
        {
            throw new InvalidOperationException("Chỉ đơn đang giao mới được hoàn thành.");
        }

        Status = OrderStatus.Completed;
    }
}
