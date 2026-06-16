
using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Carts;
using Demo_web_MVC.Models.ViewModel.Product;
using Microsoft.EntityFrameworkCore;
using Demo_web_MVC.Models.ViewModel;
using System;
namespace Demo_web_MVC.Repository.Carts
{
    public class CartRepository : ICartRepository
    {
        public readonly AppDatabase _context;
        public CartRepository(AppDatabase context)
        {
            _context = context;
        }
        public async Task<bool> AddToCartAsync(int userId, int variantId, int quantity)
        {
            if (quantity <= 0)
                throw new Exception("Số lượng phải lớn hơn 0.");

            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == variantId);

            if (variant == null)
                throw new Exception("Biến thể sản phẩm không tồn tại.");

            if (variant.Stock < quantity)
                throw new Exception("Số lượng vượt quá tồn kho.");

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    Status = "Active"
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.VariantId == variantId);

            if (cartItem != null)
            {
                var totalQuantity = cartItem.Quantity + quantity;

                if (totalQuantity > variant.Stock)
                    throw new Exception("Tổng số lượng trong giỏ vượt quá tồn kho.");

                cartItem.Quantity = totalQuantity;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    VariantId = variantId,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<CartItemViewModel>> GetCartItemsAsync(int userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
                return new List<CartItemViewModel>();
            var cartItems = await _context.CartItems.AsNoTracking()
                .Where(ci => ci.CartId == cart.Id)
                .Select(ci => new CartItemViewModel
                {
                    Id = ci.Id,
                    CartId = ci.CartId,
                    VariantId = ci.VariantId,
                    Quantity = ci.Quantity,
                    ProductName = ci.Variant.Product.Name,
                    Brand = ci.Variant.Product.Brand,
                    Size = ci.Variant.Size,
                    Color = ci.Variant.Color,
                    Price = ci.Variant.Price,
                    ImageUrl = ci.Variant.ProductVariantImages
                        .Where(pvi => pvi.VariantId == ci.VariantId)
                        .Select(pvi => pvi.Url)
                        .FirstOrDefault()
                })
                .ToListAsync();
            return cartItems;
        }
        public async Task<bool> RemoveItemAsync(int userid, int cartItemId)
        {
            var cart = await _context.Carts.AsNoTracking()
                 .FirstOrDefaultAsync(c => c.UserId == userid);
            if (cart == null)
            {
                return false;
            }


            var cartItems = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == cartItemId && c.CartId == cart.Id);
            if (cartItems == null)
            {
                return false;
            }
            _context.CartItems.Remove(cartItems);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<CartItemViewModel> UpdateQuantityAsync(int userId, int cartItemId, CartItemViewModel cartItemViewModel)
        {

            var cart = await _context.Carts.AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                throw new Exception("Giỏ hàng không tồn tại.");
            }
            var cartItem = await _context.CartItems.Include(ci => ci.Variant)
                                  .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.CartId == cart.Id);
            if (cartItem == null)
            {
                throw new Exception("Mục giỏ hàng không tồn tại.");
            }
            if (cartItemViewModel.Quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cartItemViewModel.Quantity), "Số lượng phải lớn hơn 0.");
            }
            cartItem.Quantity = cartItemViewModel.Quantity;
            cartItem.Variant.Price = cartItemViewModel.Price; 
            await _context.SaveChangesAsync();
            return new CartItemViewModel
            {
                Id = cartItem.Id,   
                CartId = cartItem.CartId,         
                VariantId = cartItem.VariantId,   
                Quantity = cartItem.Quantity,
                Price = cartItem.Variant.Price
            };
        }
    }
}

