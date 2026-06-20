using Demo_web_MVC.Controllers;
using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Address;
using Demo_web_MVC.Models.ViewModel.Carts;
using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Repository.Addresss;
using MailKit.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Demo_web_MVC.Repository.Oder
{
    public class OderRepository: IOderRepository
    {
        public readonly AppDatabase _context;
        public readonly ILogger<OderRepository> _logger;  
        public readonly IAddressRepository _addressRepository;
        public OderRepository(AppDatabase context, ILogger<OderRepository> logger, IAddressRepository addressRepository)
        {
            _context = context;
            _logger = logger;
            _addressRepository = addressRepository;
        }
        public async Task<List<int>> CreateOrderFromCartAsync(
    int userId,
    string paymentMethod,
    List<int> selectedCartItemIds)
        {
            var cart = await _context.Carts
                .Where(c => c.UserId == userId && c.Status == "active")
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Variant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync();

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                throw new InvalidOperationException("No active cart found for the user.");
            }

            var selectedItems = cart.CartItems
                .Where(ci => selectedCartItemIds.Contains(ci.Id)
                             && ci.Variant != null
                             && ci.Variant.Product != null
                             && ci.Variant.Product.SellerId != null)
                .ToList();

            if (!selectedItems.Any())
            {
                throw new InvalidOperationException("No selected items to checkout.");
            }

            foreach (var item in selectedItems)
            {
                if (item.Variant.Stock <= 0)
                {
                    throw new InvalidOperationException(
                        $"Sản phẩm {item.Variant.Product.Name} đã hết hàng."
                    );
                }

                if (item.Quantity > item.Variant.Stock)
                {
                    throw new InvalidOperationException(
                        $"Sản phẩm {item.Variant.Product.Name} chỉ còn {item.Variant.Stock} sản phẩm trong kho."
                    );
                }
            }

            var payment = Enum.TryParse(paymentMethod, out PaymentMethod method)
                ? method
                : PaymentMethod.COD;

            var itemsBySeller = selectedItems
                .GroupBy(ci => ci.Variant.Product.SellerId!.Value)
                .ToList();

            var createdOrderIds = new List<int>();

            foreach (var sellerGroup in itemsBySeller)
            {
                var totalAmount = sellerGroup.Sum(ci => ci.Quantity * ci.Variant.Price);

                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending,
                    PaymentMethod = payment,
                    CreatedAt = DateTime.Now
                };

                _context.Orders.Add(order);

                // Save trước để lấy order.Id
                await _context.SaveChangesAsync();

                createdOrderIds.Add(order.Id);

                foreach (var item in sellerGroup)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        Price = item.Variant.Price
                    };

                    _context.OrderItems.Add(orderItem);
                    item.Variant.Stock -= item.Quantity;
                }

                var orderLog = new OrderLog
                {
                    OrderId = order.Id,
                    PreviousStatus = null,
                    Status = "Pending",
                    ChangeType = "CREATE_ORDER",
                    ActionBy = userId.ToString(),
                    Reason = "User created order from cart",
                    AdditionalInfo = $"PaymentMethod: {order.PaymentMethod}, TotalAmount: {order.TotalAmount}",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.OrderLogs.Add(orderLog);

                _context.Notifications.Add(new Notification
                {
                    UserId = userId,
                    Title = "Đặt hàng thành công",
                    Content = $"Đơn hàng #{order.Id} đã được tạo thành công.",
                    Type = "ORDER_CREATED",
                    ReferenceId = order.Id,
                    Url = $"/Oder/Details?orderId={order.Id}",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

                _context.Notifications.Add(new Notification
                {
                    UserId = sellerGroup.Key,
                    Title = "Có đơn hàng mới",
                    Content = $"Bạn có đơn hàng mới #{order.Id}.",
                    Type = "NEW_ORDER",
                    ReferenceId = order.Id,
                    Url = $"/Seller/DetailsOrder?orderId={order.Id}",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

                await AddNotificationsForAdminsAsync(
                    "Có đơn hàng mới",
                    $"Khách hàng vừa tạo đơn hàng #{order.Id}.",
                    "NEW_ORDER_ADMIN",
                    order.Id,
                    $"/Admin/OrderDetail?orderId={order.Id}"
                );
            }

            _context.CartItems.RemoveRange(selectedItems);
            await _context.SaveChangesAsync();

            return createdOrderIds;
        }
        public async Task<Order> GetOrderByIdAsync(  int orderId)
        {
            var order = await _context.Orders
         .Where(o => o.Id == orderId)
         .Include(o => o.OrderItems)  // Đảm bảo rằng các sản phẩm trong đơn hàng được bao gồm
         .FirstOrDefaultAsync();
            if (order == null)
            {
                _logger.LogError("đơn hàng không hợp lệ");
                throw new InvalidOperationException("Đơn hàng không hợp lệ");
            }
            var result = new Order
            {
                Id = orderId,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                PaymentMethod = order.PaymentMethod,
            };
            return result;
        }
        public async Task<OderViewModel?> GetOrderDetailAsync(int userId, int orderId)
        {
            var result = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Select(o => new OderViewModel
                {
                    Id = o.Id,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CreateAt = o.CreatedAt,
                    
                    Items = o.OrderItems.Select(item => new OderItemViewModel
                    {
                        Name = item.Variant.Product.Name,
                        Price = item.Price,
                        Quantity = item.Quantity,
                        Variant = item.Variant,
                        SellerId = item.Variant.Product.SellerId,
                        Img = item.Variant.ProductVariantImages
                            .OrderBy(img => img.SortOrder)
                            .Select(img => img.Url)
                            .FirstOrDefault()
                            ?? item.Variant.Product.ProductImages
                                .Select(img => img.Url)
                                .FirstOrDefault()
                            ?? "/uploads/images/no-image.jpg"
                    }).ToList(),

                    AddressViewModels = o.User.Addresses.Select(address => new AddressViewModel
                    {
                        RecipientName = address.RecipientName,
                        PhoneNumber = address.PhoneNumber,
                        AddressLine = address.AddressLine,
                        City = address.City,
                        Country = address.Country
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                _logger.LogError("Không có order. userId={UserId}, orderId={OrderId}", userId, orderId);
                return null;
            }

            return result;
        }
        public async Task<List<OderViewModel>> GetOrdersByUserAsync(int userId)
        {
            var result = await _context.Orders.AsNoTracking()
               .Where(o =>  o.UserId == userId)
               .OrderByDescending(o => o.CreatedAt)
               .Select(o => new OderViewModel
               {
                   Id = o.Id,
                   TotalAmount = o.TotalAmount,
                   Status = o.Status,
                   CreateAt = o.CreatedAt,
                   Items = o.OrderItems.Select(item => new OderItemViewModel
                   {
                       Name = item.Variant.Product.Name,
                       Price = item.Price,
                       Quantity = item.Quantity,
                   }).ToList()
               }).ToListAsync();

            if (result.Count == 0)
            {
                _logger.LogInformation("User chưa có đơn hàng. userId={UserId}", userId);
            }

            return result;
        }
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                return false;
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;
            var previousStatus = order.Status;

            order.Status = parsedStatus;
            var orderLog = new OrderLog
            {
                OrderId = order.Id,
                PreviousStatus = previousStatus.ToString(),
                Status = parsedStatus.ToString(),
                ChangeType = GetOrderChangeType(parsedStatus),
                ActionBy = "System",
                Reason = GetOrderReason(parsedStatus),
                AdditionalInfo = null,
                CreatedAt = DateTime.Now
            };

            _context.OrderLogs.Add(orderLog);
            _context.Notifications.Add(new Notification
            {
                UserId = order.UserId,
                Title = "Cập nhật đơn hàng",
                Content = $"Đơn hàng #{order.Id} đã chuyển sang trạng thái {parsedStatus}.",
                Type = "ORDER_STATUS",
                ReferenceId = order.Id,
                Url = $"/Oder/Details?orderId={order.Id}",
                IsRead = false,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> CancelOrderAsync(int orderId, int userId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy order. orderId={OrderId}, userId={UserId}", orderId, userId);
                return false;
            }


            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            {
                _logger.LogWarning("Không thể huỷ đơn. status={Status}", order.Status);
                return false;
            }
            var previousStatus = order.Status;
            order.Status = OrderStatus.Cancelled;
            var orderLog = new OrderLog
            {
                OrderId = order.Id,
                PreviousStatus = previousStatus.ToString(),
                Status = order.Status.ToString(),
                ChangeType = "CANCEL_ORDER",
                ActionBy = userId.ToString(),
                Reason = "User cancelled order",
                AdditionalInfo = null,
                CreatedAt = DateTime.Now
            };
            _context.OrderLogs.Add(orderLog);
            var sellerId = await _context.OrderItems
                .Where(x => x.OrderId == order.Id)
                .Select(x => x.Variant.Product.SellerId)
                .FirstOrDefaultAsync();
            if (sellerId != null)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = sellerId.Value,
                    Title = "Khách đã hủy đơn",
                    Content = $"Khách đã hủy đơn hàng #{order.Id}.",
                    Type = "ORDER_CANCELLED_BY_CUSTOMER",
                    ReferenceId = order.Id,
                    Url = $"/Seller/DetailsOrder?orderId={order.Id}",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });
            }
            await AddNotificationsForAdminsAsync(
                "Khách đã hủy đơn",
                $"Khách hàng đã hủy đơn hàng #{order.Id}.",
                "ORDER_CANCELLED_BY_CUSTOMER_ADMIN",
                order.Id,
                $"/Admin/OrderDetail?orderId={order.Id}"
            );
            await _context.SaveChangesAsync();

            return true;

        }
        public async Task<decimal> CalculateOrderTotalAsync(int userId)
        {
            var result = await _context.Orders.Where(o => o.UserId == userId).SumAsync(o => o.TotalAmount);
            return result;

        }
        public async Task<CheckoutViewModel> CheckOutAsync(int userId,List<int> selectedCartItemIds)
        {
            // Lấy các sản phẩm trong giỏ hàng
            var cartItems = await _context.CartItems
                .Where(ci => selectedCartItemIds.Contains(ci.Id)&& ci.Cart.UserId == userId && ci.Cart.Status == "active")
                .Include(ci => ci.Variant)
                .ThenInclude(ci => ci.ProductVariantImages) 
                .Include(ci => ci.Variant.Product)   
                .ToListAsync();
            if (cartItems.Count > 0)
                _logger.LogInformation("có sản phẩm");
             
            var addressViewModels = await _context.Addresses
                .Where(a => a.UserId == userId)
                .Select(a => new AddressViewModel
                {
                    Id = a.Id,
                    RecipientName = a.RecipientName,
                    PhoneNumber = a.PhoneNumber,
                    AddressLine = a.AddressLine,
                    City = a.City,
                    Country = a.Country,
                    IsDefault = a.IsDefault
                })
                .ToListAsync();

            
            var totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Variant.Price);

            // Tạo model CheckoutViewModel
            var model = new CheckoutViewModel
            {
                CartItems = cartItems.Select(ci => new CartItemViewModel
                {
                    Id = ci.Id,
                    ProductName = ci.Variant.Product.Name,
                    Price = ci.Variant.Price,
                    Quantity = ci.Quantity,
                    ImageUrl = ci.Variant.ProductVariantImages.FirstOrDefault()?.Url  
                }).ToList(),
                AddressViewModels = addressViewModels,
                TotalAmount = totalAmount,
                SelectedAddressId = addressViewModels.FirstOrDefault(a => a.IsDefault)?.Id,   
                PaymentMethod = PaymentMethod.COD
            };
            return model;
        }
        public async Task RemoveCartItemsAsync(List<int> selectedCartItemIds, int userId)
        {
            // Lấy giỏ hàng của người dùng với trạng thái "active"
            var cart = await _context.Carts
                .Where(c => c.UserId == userId && c.Status == "active")
                .Include(c => c.CartItems) // Bao gồm các CartItems của giỏ hàng
                .FirstOrDefaultAsync();

            if (cart != null)
            {
                // Lọc các CartItem mà ID có trong selectedCartItemIds
                var selectedItems = cart.CartItems.Where(ci => selectedCartItemIds.Contains(ci.Id)).ToList();

                // Xóa chỉ các CartItem đã chọn
                _context.CartItems.RemoveRange(selectedItems);
                await _context.SaveChangesAsync(); // Lưu thay đổi vào database
            }
        }
        public async Task<List<OderViewModel>> GetAllOrderIdsAsync(int orderId) 
        {
            
            var orderIds = await _context.Orders
                .AsNoTracking()

                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OderViewModel
                {
                    Id = o.Id,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CreateAt = o.CreatedAt,

                    Items = o.OrderItems.Select(item => new OderItemViewModel
                    {
                        Name = item.Variant.Product.Name,
                        Price = item.Price,
                        Quantity = item.Quantity,

                        Img = item.Variant.ProductVariantImages
                            .OrderBy(img => img.SortOrder)
                            .Select(img => img.Url)
                            .FirstOrDefault()
                            ?? item.Variant.Product.ProductImages
                                .Select(img => img.Url)
                                .FirstOrDefault()
                            ?? "/uploads/images/no-image.jpg"
                    }).ToList()
                })
                .ToListAsync();


            return orderIds;
        }
        public async Task<List<OderViewModel>> GetAllOrders(int userId, string status)
        {
            IQueryable<Order> ordersQuery = _context.Orders.Where(o => o.UserId == userId);

            if (status != "All")
            {
                ordersQuery = ordersQuery.Where(o => o.Status == (OrderStatus)Enum.Parse(typeof(OrderStatus), status));
            }
            var orders = await ordersQuery
                .AsNoTracking()
                
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OderViewModel
                {
                    Id = o.Id,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    CreateAt = o.CreatedAt,
                     
                    Items = o.OrderItems.Select(item => new OderItemViewModel
                    {
                        Name = item.Variant.Product.Name,
                        Price = item.Price,
                        Quantity = item.Quantity,

                        Img = item.Variant.ProductVariantImages
                            .OrderBy(img => img.SortOrder)
                            .Select(img => img.Url)
                            .FirstOrDefault()
                            ?? item.Variant.Product.ProductImages
                                .Select(img => img.Url)
                                .FirstOrDefault()
                            ?? "/uploads/images/no-image.jpg"
                    }).ToList()
                })
                .ToListAsync();

            return orders;
        }
        //người bán
        public async Task<bool> DeleteOrderAsync(int orderId, int sellerId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy đơn hàng với orderId={OrderId}", orderId);
                return false;
            }

            // Kiểm tra đơn này có sản phẩm của seller không
            var isSellerOrder = order.OrderItems.All(oi =>
                oi.Variant.Product.SellerId == sellerId);

            if (!isSellerOrder)
            {
                _logger.LogWarning(
                    "Seller {SellerId} không có quyền hủy orderId={OrderId}",
                    sellerId,
                    orderId);

                return false;
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                _logger.LogWarning("Đơn hàng orderId={OrderId} đã bị hủy trước đó.", orderId);
                return false;
            }

            var previousStatus = order.Status;

            order.Status = OrderStatus.Cancelled;

            var orderLog = new OrderLog
            {
                OrderId = order.Id,
                PreviousStatus = previousStatus.ToString(),
                Status = order.Status.ToString(),
                ChangeType = "CANCEL_ORDER",
                ActionBy = $"SellerId:{sellerId}",
                Reason = "Seller hủy đơn hàng",
                AdditionalInfo = null,
                CreatedAt = DateTime.Now
            };

            _context.OrderLogs.Add(orderLog);
            _context.Notifications.Add(new Notification
            {
                UserId = order.UserId,
                Title = "Đơn hàng bị hủy",
                Content = $"Đơn hàng #{order.Id} đã bị người bán hủy.",
                Type = "ORDER_CANCELLED",
                ReferenceId = order.Id,
                Url = $"/Oder/Details?orderId={order.Id}",
                IsRead = false,
                CreatedAt = DateTime.Now
            });
            await AddNotificationsForAdminsAsync(
                "Seller đã hủy đơn",
                $"Seller #{sellerId} đã hủy đơn hàng #{order.Id}.",
                "ORDER_CANCELLED_BY_SELLER_ADMIN",
                order.Id,
                $"/Admin/OrderDetail?orderId={order.Id}"
            );
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Seller {SellerId} đã hủy đơn hàng orderId={OrderId}.",
                sellerId,
                orderId);

            return true;
        }
        public async Task<bool> CreateAsync(int orderId, int sellerId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogError("Không tìm thấy đơn hàng với orderId={OrderId}", orderId);
                return false;
            }

            // Kiểm tra đơn này có sản phẩm của seller không
            var isSellerOrder = order.OrderItems.All(oi =>
                oi.Variant.Product.SellerId == sellerId);

            if (!isSellerOrder)
            {
                _logger.LogWarning(
                    "Seller {SellerId} không có quyền cập nhật orderId={OrderId}",
                    sellerId,
                    orderId);

                return false;
            }

            if (order.Status != OrderStatus.Pending &&
                order.Status != OrderStatus.Confirmed)
            {
                _logger.LogWarning(
                    "Không thể nhận đơn. Đơn hàng không ở trạng thái hợp lệ. orderId={OrderId}, Status={Status}",
                    orderId,
                    order.Status);

                return false;
            }

            var previousStatus = order.Status;

            order.Status = OrderStatus.Shipping;

            var orderLog = new OrderLog
            {
                OrderId = order.Id,
                PreviousStatus = previousStatus.ToString(),
                Status = order.Status.ToString(),
                ChangeType = "SHIPPING_ORDER",
                ActionBy = $"SellerId:{sellerId}",
                Reason = "Seller nhận đơn và chuyển sang Shipping",
                AdditionalInfo = null,
                CreatedAt = DateTime.Now
            };

            _context.OrderLogs.Add(orderLog);
            _context.Notifications.Add(new Notification
            {
                UserId = order.UserId,
                Title = "Đơn hàng đang giao",
                Content = $"Đơn hàng #{order.Id} đang được vận chuyển.",
                Type = "ORDER_SHIPPING",
                ReferenceId = order.Id,
                Url = $"/Oder/Details?orderId={order.Id}",
                IsRead = false,
                CreatedAt = DateTime.Now
            });
            await AddNotificationsForAdminsAsync(
                "Seller đã nhận đơn",
                $"Seller #{sellerId} đã chuyển đơn hàng #{order.Id} sang đang giao.",
                "ORDER_SHIPPING_ADMIN",
                order.Id,
                $"/Admin/OrderDetail?orderId={order.Id}"
            );
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Seller {SellerId} đã chuyển orderId={OrderId} sang Shipping.",
                sellerId,
                orderId);

            return true;
        }
        private async Task AddNotificationsForAdminsAsync(
    string title,
    string content,
    string type,
    int? referenceId = null,
    string? url = null)
        {
            var adminIds = await _context.Users
                .Where(u => u.UserRoles.Any(ur => ur.Role.Code == "ADMIN"))
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var adminId in adminIds)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = adminId,
                    Title = title,
                    Content = content,
                    Type = type,
                    ReferenceId = referenceId,
                    Url = url,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });
            }
        }
        private string GetOrderReason(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Order created",
                OrderStatus.Confirmed => "Order confirmed",
                OrderStatus.Shipping => "Order shipping",
                OrderStatus.Completed => "Order completed",
                OrderStatus.Cancelled => "Order cancelled",
                _ => "Order status updated"
            };
        }
        private string GetOrderChangeType(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "CREATE_ORDER",
                OrderStatus.Confirmed => "CONFIRM_ORDER",
                OrderStatus.Shipping => "SHIPPING_ORDER",
                OrderStatus.Completed => "COMPLETE_ORDER",
                OrderStatus.Cancelled => "CANCEL_ORDER",
                _ => "UPDATE_STATUS"
            };
        }

        public async Task<List<Order>> GetShippingOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .Where(o => o.Status == OrderStatus.Shipping)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}
