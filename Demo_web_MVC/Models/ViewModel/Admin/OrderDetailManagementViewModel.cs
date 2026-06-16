using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Models.ViewModel.Address;

namespace Demo_web_MVC.Models.ViewModel.Admin
{
    public class OrderDetailManagementViewModel
    {
        public AddressViewModel? Address { get; set; } 
        public OderViewModel Order { get; set; } = new();

        public User User { get; set; } = new();
        
        public List<OderItemViewModel> OrderDetails { get; set; } = new();

    }
}
