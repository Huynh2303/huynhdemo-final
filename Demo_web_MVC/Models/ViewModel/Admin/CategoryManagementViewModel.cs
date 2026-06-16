using Demo_web_MVC.Models.ViewModel.Category;

namespace Demo_web_MVC.Models.ViewModel.Admin
{
    public class CategoryManagementViewModel
    {
        public int TotalCategories { get; set; }

        public PaginatedList<CategoryViewModel>? Categories { get; set; }
        

        // Form thêm / sửa
        public int? Id { get; set; }

        public string? Name { get; set; }

        public int? ParentId { get; set; }

        // Dropdown category cha
        public List<CategoryViewModel> ParentCategories { get; set; } = new();

    }
}
