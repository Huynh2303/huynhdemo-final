namespace Demo_web_MVC.Models.ViewModel.Category
{
    public class CategoryViewModel
    {
        public int? Id { get; set; }
        public string? Name { get; set; } = null!;
        public int? ParentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
