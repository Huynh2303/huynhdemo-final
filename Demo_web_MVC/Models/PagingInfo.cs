namespace Demo_web_MVC.Models
{
    public class PagingInfo
    {
        public int CurrentPage { get; set; }  // Trang hiện tại
        public int TotalPages { get; set; }   // Tổng số trang
        public int TotalItems { get; set; }   // Tổng số mục
        public int ItemsPerPage { get; set; } // Số mục mỗi trang
    }
}
