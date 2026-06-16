using Demo_web_MVC.Models.ViewModel.Address;
using Demo_web_MVC.Models.ViewModel.Carts;

namespace Demo_web_MVC.Models.ViewModel.Oder
{
    public class OderViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int  Quatity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime? CreateAt { get; set; } = DateTime.Now;
        
        public List<OderItemViewModel> Items { get; set; } = new();
        public List<AddressViewModel> AddressViewModels { get; set; } = new();
        public List<CartItemViewModel> cartItems { get; set; } = new();
        public PaymentMethod PaymentMethod { get; set; }
        public string? user { get; set; }
        public FraudAnalysisViewModel? FraudAnalysis { get; set; }
    }
}
