using Demo_web_MVC.Models.ViewModel.Carts;

namespace Demo_web_MVC.Service.Cart
{
    public interface ICartService
    {
        Task<bool> AddToCartAsync(int userId, int variantId, int quantity);
        Task<List<CartItemViewModel>> GetCartItems(int userId);
        Task<bool> RemoveItemAsync(int userId, int cartItemId);
        Task < CartItemViewModel > UpdateQuantityAsync(int userId, int cartItemId, CartItemViewModel cartItemViewModel);
        
    }
}
