using Demo_web_MVC.Models.ViewModel.Address;
using Demo_web_MVC.Models.ViewModel.Oder;

namespace Demo_web_MVC.Models.ViewModel.Dashboard
{
    public class DetailsOrderDashboardViewmodel
    {
        
        public int OrderId { get; set; }
        public string? Email { get; set; }

        

        public string? OrderStatus { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }

        public AddressViewModel? AddressView { get; set; }

        public FraudAnalysisViewModel? FraudAnalysis { get; set; }

        public List<OderItemViewModel> OderItemViews { get; set; } = new List<OderItemViewModel>();

        
    }
}