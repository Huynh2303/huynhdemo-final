

using Demo_web_MVC.Models.ViewModel.Carts;
namespace Demo_web_MVC.Repository.Carts
    
{
    public interface ICartRepository
    {
        Task<bool> AddToCartAsync(int userId, int variantId, int quantity);
        Task<List<CartItemViewModel>> GetCartItemsAsync(int userId);
        Task<CartItemViewModel> UpdateQuantityAsync(int userId, int cartItemId, CartItemViewModel cartItemViewModel);
        Task<bool> RemoveItemAsync(int userId, int cartItemId); 
    }
}
