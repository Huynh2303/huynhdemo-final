namespace Demo_web_MVC.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; } // FK

        public string Url { get; set; } = null!;

        public int SortOrder { get; set; }

        // Navigation property
        public Product Product { get; set; } = null!;
    }
}
