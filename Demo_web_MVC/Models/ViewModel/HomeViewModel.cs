using Demo_web_MVC.Controllers;
using Demo_web_MVC.Models.ViewModel.Address;

namespace Demo_web_MVC.Models.ViewModel
{
    public class HomeViewModel
    {
        public List<AddressViewModel> address { get; set; }  = new List<AddressViewModel>();
    }
}
