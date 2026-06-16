using Demo_web_MVC.Models.ViewModel.Address;
using Demo_web_MVC.Models.ViewModel.Carts;

namespace Demo_web_MVC.Models.ViewModel.Oder
{
    public class CheckoutViewModel
    {
        public List<AddressViewModel> AddressViewModels { get; set; } = new();

        // Danh sách sản phẩm được chọn từ giỏ hàng
        public List<CartItemViewModel> CartItems { get; set; } = new();

        // Tổng tiền các sản phẩm được chọn
        public decimal TotalAmount { get; set; }

        // Địa chỉ user chọn
        public int? SelectedAddressId { get; set; }

        // Phương thức thanh toán user chọn
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

        // Danh sách CartItemId được chọn từ giỏ hàng
        public List<int> SelectedCartItemIds { get; set; } = new();
    }
}
