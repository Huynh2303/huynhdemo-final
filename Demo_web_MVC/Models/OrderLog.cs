using System;
using System.Collections.Generic;

namespace Demo_web_MVC.Models;

public partial class OrderLog
{
    public int Id { get; set; }  // Khóa chính của log
    public int OrderId { get; set; }  // Liên kết với bảng Orders
    public Order? Order { get; set; }  // Mối quan hệ với Order

    public string? Status { get; set; }  // Trạng thái hiện tại của đơn hàng
    public string? PreviousStatus { get; set; }  // Trạng thái trước khi thay đổi
    public DateTime CreatedAt { get; set; }  // Thời gian khi trạng thái thay đổi
    public DateTime UpdatedAt { get; set; }  // Thời gian cập nhật trạng thái

    public string? ActionBy { get; set; }  // Người thực hiện thay đổi trạng thái
    public string? Reason { get; set; }  // Lý do thay đổi trạng thái
    public string? ChangeType { get; set; }  // Loại thay đổi trạng thái
    public string? AdditionalInfo { get; set; }  // Thông tin bổ sung (mã vận đơn, số lượng, v.v.)
}
