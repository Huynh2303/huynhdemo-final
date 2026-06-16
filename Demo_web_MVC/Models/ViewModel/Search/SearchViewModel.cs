using Demo_web_MVC.Models.ViewModel.Product;

namespace Demo_web_MVC.Models.ViewModel.Search
{
    public class SearchViewModel
    {
        // Từ khóa tìm kiếm mà người dùng nhập vào
        public string? SearchQuery { get; set; }

        // Kết quả tìm kiếm (có thể là một danh sách các đối tượng tùy vào loại dữ liệu)
        public List<ProductViewModel>? ProductVMResults { get; set; }

        // Tổng số kết quả tìm kiếm
        public int TotalResults { get; set; }

        // Các lựa chọn lọc và phân loại (nếu có)
        public string? SortBy { get; set; }
        public string? FilterByCategory { get; set; }

        // Trạng thái tìm kiếm và lỗi (nếu có)
        public string? SearchStatus { get; set; }
        public string? ErrorMessage { get; set; }
        public PagingInfo? PagingInfo { get; set; }
    }
}