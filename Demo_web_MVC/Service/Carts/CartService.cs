using Demo_web_MVC.Models.ViewModel.Carts;
using Demo_web_MVC.Repository.Carts;

namespace Demo_web_MVC.Service.Cart
{
    public class CartService : ICartService
    {
        public readonly ICartRepository _cartRepository;
        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }
        public async Task<bool> AddToCartAsync(int userId, int variantId, int quantity)
        {
            if (quantity <= 0)
                throw new Exception("Số lượng phải lớn hơn 0.");
            if (variantId <= 0)
                throw new Exception("Biến thể sản phẩm không hợp lệ.");
            if (userId <= 0)
                throw new Exception("Người dùng không hợp lệ.");
            return await _cartRepository.AddToCartAsync(userId, variantId, quantity);
        }
        public async Task<List<CartItemViewModel>> GetCartItems(int userId)
        {
            if (userId <= 0)
                throw new Exception("Người dùng không hợp lệ.");
            return await _cartRepository.GetCartItemsAsync(userId);
        }
        public async Task<bool> RemoveItemAsync(int userId, int cartItemId)
        {
            if (cartItemId <= 0)
                throw new Exception("Biến thể sản phẩm không hợp lệ.");
            if (userId <= 0)
                throw new Exception("Người dùng không hợp lệ.");
            return await _cartRepository.RemoveItemAsync(userId, cartItemId);
        }
        public async Task<CartItemViewModel> UpdateQuantityAsync(int userId, int cartItemId, CartItemViewModel cartItemViewModel)
        {
            if (cartItemId <= 0)
                throw new Exception("Biến thể sản phẩm không hợp lệ.");
            if (userId <= 0)
                throw new Exception("Người dùng không hợp lệ.");
            if (cartItemViewModel.Quantity <= 0)
                throw new Exception("Số lượng phải lớn hơn 0.");
            return await _cartRepository.UpdateQuantityAsync(userId, cartItemId, cartItemViewModel);
        }
    }
}
