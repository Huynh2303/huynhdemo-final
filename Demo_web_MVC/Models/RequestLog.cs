namespace Demo_web_MVC.Models
{
    public class RequestLog
    {
        public int Id { get; set; } // Khoá chính
        public int UserId { get; set; } // ID của người dùng
        public string? UserRole { get; set; } // Vai trò của người dùng
        public string? IpAddress { get; set; } // Địa chỉ IP của người dùng
        public string? ActionType { get; set; } // Loại hành động (login, payment, order_creation, v.v.)
        public DateTime Timestamp { get; set; } // Thời gian request được gửi
        public string? PaymentMethod { get; set; } // Phương thức thanh toán (COD, Credit Card, v.v.)
        public List<OrderItem>? OrderItems { get; set; } // Danh sách các mục trong đơn hàng
        public decimal TotalAmount { get; set; } // Tổng số tiền của đơn hàng
        public string? RequestPath { get; set; } // Đường dẫn của request
        public string? RequestMethod { get; set; } // Phương thức HTTP của request (GET, POST, PUT, DELETE)
        public string? RequestBody { get; set; } // Nội dung của request body (thường là JSON)
        public string? Flags { get; set; } // Các cờ đánh dấu hành vi bất thường (JSON)
        public int ResponseStatus { get; set; } // Trạng thái phản hồi từ server (200 OK, 400 Bad Request, v.v.)
    }
}

