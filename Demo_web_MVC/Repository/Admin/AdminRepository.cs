using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel;
using Demo_web_MVC.Models.ViewModel.Address;
using Demo_web_MVC.Models.ViewModel.Admin;
using Demo_web_MVC.Models.ViewModel.Category;
using Demo_web_MVC.Models.ViewModel.Dashboard;
using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Repository.Dashboard;
using Demo_web_MVC.Repository.Paging;
using Microsoft.EntityFrameworkCore;

namespace Demo_web_MVC.Repository.Admin
{
    public class AdminRepository: IAdminRepository
    {
        private readonly AppDatabase _context;
        private readonly ILogger<AdminRepository> _logger;
        private readonly IPagingReponsitory _pagingReponsitory;
        public AdminRepository(AppDatabase context, ILogger<AdminRepository> logger, IPagingReponsitory pagingReponsitory)
        {
            _context = context;
            _logger = logger;
            _pagingReponsitory = pagingReponsitory;
        }
        public async Task<AdminViewModel> GetAdminDashboardAsync()
        {
            try
            {
                _logger.LogInformation("Bắt đầu lấy dữ liệu dashboard admin...");

                // Tổng doanh thu
                var totalRevenue = await _context.Orders
                    .Where(o => o.Status == OrderStatus.Completed)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

                // Tổng đơn hàng
                var totalOrders = await _context.Orders
                    .CountAsync();

                // Tổng sản phẩm
                var totalProducts = await _context.Products
                    .CountAsync();

                // Tổng người dùng
                var totalUsers = await _context.Users
                    .CountAsync();

                // Đơn hàng gần đây
                var recentOrders = await _context.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(10)
                    .Select(o => new OderViewModel
                    {
                        Id = o.Id,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        CreateAt = o.CreatedAt,
                        user = o.User.FullName,

                        FraudAnalysis = _context.FraudAnalyses
                            .Where(f => f.OrderId == o.Id)
                            .OrderByDescending(f => f.CreatedAt)
                            .Select(f => new FraudAnalysisViewModel
                            {
                                Id = f.Id,
                                OrderId = f.OrderId,
                                RiskScore = f.RiskScore,
                                RiskLevel = f.RiskLevel,
                                RiskReasons = f.RiskReasons,
                                ModelName = f.ModelName,
                                CreatedAt = f.CreatedAt
                            })
                            .FirstOrDefault(),

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

                var result = new AdminViewModel
                {
                    TotalRevenue = totalRevenue,
                    TotalOrders = totalOrders,
                    TotalProducts = totalProducts,
                    TotalUsers = totalUsers,

                    oderViewModels = recentOrders
                };

                _logger.LogInformation("Lấy dữ liệu dashboard admin thành công.");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi khi lấy dashboard admin: {Message}",
                    ex.Message);

                throw;
            }
        }
        public async Task<OderManagementViewModel> GetOrderManagementAsync(int page, int pageSize)
        {
            var ordersQuery = _context.Orders
                .AsNoTracking();

            var totalOrders = await ordersQuery.CountAsync();

            var pendingOrders = await ordersQuery
                .CountAsync(o => o.Status == OrderStatus.Pending);

            var cancelledOrders = await ordersQuery
                .CountAsync(o => o.Status == OrderStatus.Cancelled);

            var revenue = await ordersQuery
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);

           var orders = ordersQuery
    .OrderByDescending(o => o.CreatedAt)
    .Select(o => new OderViewModel
    {
        Id = o.Id,
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        CreateAt = o.CreatedAt,
        PaymentMethod = o.PaymentMethod,

        user = o.User.FullName ?? o.User.Username,

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
    });
            var pagedOrders = await _pagingReponsitory.GetPagedDataAsync(
                orders,
                page,
                pageSize
            );
            return new OderManagementViewModel
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                CancelledOrders = cancelledOrders,
                Revenue = revenue,
                Orders = pagedOrders
            };
        }
        public async Task<ProductManagementViewModel> GetProductManagementAsync(int page,int pageSize)
        {
            var productsQuery = _context.Products
                .AsNoTracking();

            // Dashboard stats
            var totalProducts = await productsQuery.CountAsync();

            var totalCategories = await _context.Categories
                .CountAsync();

            var lowStockProducts = await productsQuery
                .CountAsync(p => p.ProductVariants
                    .Sum(v => v.Stock) > 0
                    && p.ProductVariants.Sum(v => v.Stock) < 10);

            var outOfStockProducts = await productsQuery
                .CountAsync(p => p.ProductVariants
                    .Sum(v => v.Stock) <= 0);

            // Query product list
            var productVmQuery = productsQuery
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,

                    Name = p.Name,

                    Description = p.Description,

                    Brand = p.Brand,

                    CreatedAt = p.CreatedAt,

                    imageUrl = p.ProductImages
                        .OrderBy(i => i.SortOrder)
                        .Select(i => i.Url)
                        .ToList(),

                    Categories = new List<CategoryViewModel>
                    {
                new CategoryViewModel
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name
                }
                    },

                    Variants = p.ProductVariants
                        .Select(v => new ProductVariantsViewModel
                        {
                            Id = v.Id,

                            Price = v.Price,

                            Stock = v.Stock
                        })
                        .ToList()
                });

            var totalCount = await productVmQuery.CountAsync();

            var items = await productVmQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedProducts = new PaginatedList<ProductViewModel>(
                items,
                totalCount,
                page,
                pageSize
            );

            return new ProductManagementViewModel
            {
                TotalProducts = totalProducts,

                LowStockProducts = lowStockProducts,

                OutOfStockProducts = outOfStockProducts,

                TotalCategories = totalCategories,

                Products = pagedProducts
            };
        }
        public async Task<UserManagementViewModel> GetUserManagementAsync(int page, int pageSize)
        {
            var now = DateTime.Now;

            var usersQuery = _context.Users
                .AsNoTracking();

            var totalUsers = await usersQuery.CountAsync();

            var activeUsers = await usersQuery
                .CountAsync(u => u.IsActive);

            var lockedUsers = await usersQuery
                .CountAsync(u => u.LockoutUntil != null && u.LockoutUntil > now);

            var userVmQuery = usersQuery
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserItemViewModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LockoutUntil = u.LockoutUntil,

                    IsLocked = u.LockoutUntil != null && u.LockoutUntil > now,

                    RoleName = u.UserRoles
                        .Select(ur => ur.Role.Name)
                        .FirstOrDefault() ?? "Customer"
                });

            var pagedUsers = await _pagingReponsitory.GetPagedDataAsync(
                userVmQuery,
                page,
                pageSize
            );

            return new UserManagementViewModel
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                LockedUsers = lockedUsers,
                Users = pagedUsers
            };
        }
        public async Task<CategoryManagementViewModel> GetCategoryManagementAsync(int page, int pageSize)
        {
            var categoriesQuery = _context.Categories
                .AsNoTracking();

            var totalCategories = await categoriesQuery.CountAsync();

            var categoryVmQuery = categoriesQuery
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId,
                    CreatedAt = c.CreatedAt
                });

            var pagedCategories = await _pagingReponsitory.GetPagedDataAsync(
                categoryVmQuery,
                page,
                pageSize
            );

            return new CategoryManagementViewModel
            {
                TotalCategories = totalCategories,
                Categories = pagedCategories
            };
        }
        public async Task<OrderDetailManagementViewModel?> GetOrderDetailManagementAsync(int orderId)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == orderId)
                .Select(o => new OrderDetailManagementViewModel
                {
                    Order = new OderViewModel
                    {
                        Id = o.Id,
                        Name = o.User.FullName ?? o.User.Username,
                        TotalAmount = o.TotalAmount,
                        PaymentMethod = o.PaymentMethod,
                        Status = o.Status,
                        CreateAt = o.CreatedAt,
                        user = o.User.FullName ?? o.User.Username,

                        Items = o.OrderItems.Select(item => new OderItemViewModel
                        {
                            OrderId = item.OrderId,
                            VariantId = item.VariantId,
                            Name = item.Variant.Product.Name,
                            Price = item.Price,
                            Quantity = item.Quantity,

                            Img = item.Variant.ProductVariantImages
                                .OrderBy(img => img.SortOrder)
                                .Select(img => img.Url)
                                .FirstOrDefault()
                                ?? item.Variant.Product.ProductImages
                                    .OrderBy(img => img.SortOrder)
                                    .Select(img => img.Url)
                                    .FirstOrDefault()
                                ?? "/uploads/images/no-image.jpg",

                            Variant = item.Variant
                        }).ToList()
                    },

                    User = o.User,
                    Address = o.User.Addresses
                        .Where(a => a.IsDefault)
                        .Select(a => new AddressViewModel
                        {
                            Id = a.Id,
                            AddressLine = a.AddressLine,
                            City = a.City,
                            Country = a.Country,
                            IsDefault = a.IsDefault,
                            RecipientName = a.RecipientName,
                            PhoneNumber = a.PhoneNumber
                        })
                        .FirstOrDefault(),
                    OrderDetails = o.OrderItems.Select(item => new OderItemViewModel
                    {
                        OrderId = item.OrderId,
                        VariantId = item.VariantId,
                        Name = item.Variant.Product.Name,
                        Price = item.Price,
                        Quantity = item.Quantity,

                        Img = item.Variant.ProductVariantImages
                            .OrderBy(img => img.SortOrder)
                            .Select(img => img.Url)
                            .FirstOrDefault()
                            ?? item.Variant.Product.ProductImages
                                .OrderBy(img => img.SortOrder)
                                .Select(img => img.Url)
                                .FirstOrDefault()
                            ?? "/uploads/images/no-image.jpg",

                        Variant = item.Variant
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return order;
        }
        public async Task<ProductManagerDetailViewModel?> GetProductManagerDetailAsync(int productId)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == productId)
                .Select(p => new ProductManagerDetailViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Brand = p.Brand,

                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null,

                    CreatedAt = p.CreatedAt,
                    IsDeleted = p.IsDeleted,

                    ProductImages = p.ProductImages
                        .Select(img => img.Url)
                        .ToList(),

                    Variants = p.ProductVariants
                        .Select(v => new ProductVariantsViewModel
                        {
                            Id = v.Id,
                            ProductId = v.ProductId,
                            Size = v.Size,
                            Color = v.Color,
                            Price = v.Price,
                            Stock = v.Stock,

                            ImageUrlsVariants = v.ProductVariantImages
                                .Select(img => img.Url)
                                .ToList()
                        })
                        .ToList(),

                    TotalVariants = p.ProductVariants.Count(),
                    TotalStock = p.ProductVariants.Sum(v => v.Stock),

                    MinPrice = p.ProductVariants.Any()
                        ? p.ProductVariants.Min(v => v.Price)
                        : 0,

                    MaxPrice = p.ProductVariants.Any()
                        ? p.ProductVariants.Max(v => v.Price)
                        : 0
                })
                .FirstOrDefaultAsync();

            return product;
        }
        public async Task<bool> DeleteProductByAdminAsync(int productId)
        {
            var product = await _context.Products
                .IgnoreQueryFilters()
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.ProductVariantImages)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return false;
            }

            var variantIds = product.ProductVariants
                .Select(v => v.Id)
                .ToList();

            var hasOrder = await _context.OrderItems
                .AnyAsync(oi => variantIds.Contains(oi.VariantId));

            if (hasOrder)
            {
                // Đã từng có đơn hàng -> chỉ ẩn product
                product.IsDeleted = true;

                // Xóa khỏi giỏ hàng để khách không checkout tiếp sản phẩm đã ẩn
                var cartItems = await _context.CartItems
                    .Where(ci => variantIds.Contains(ci.VariantId))
                    .ToListAsync();

                _context.CartItems.RemoveRange(cartItems);
            }
            else
            {
                // Chưa có đơn hàng -> admin được xóa cứng

                var cartItems = await _context.CartItems
                    .Where(ci => variantIds.Contains(ci.VariantId))
                    .ToListAsync();

                _context.CartItems.RemoveRange(cartItems);

                _context.ProductVariantImages.RemoveRange(
                    product.ProductVariants.SelectMany(v => v.ProductVariantImages)
                );

                _context.ProductImages.RemoveRange(product.ProductImages);

                _context.ProductVariants.RemoveRange(product.ProductVariants);

                _context.Products.Remove(product);
            }
            _context.Notifications.Add(new Notification
            {
                UserId = product.SellerId!.Value,
                Title = "Sản phẩm đã bị quản trị viên xử lý",
                Content = $"Sản phẩm \"{product.Name}\" đã bị admin xóa hoặc ẩn khỏi hệ thống.",
                Type = "Product",
                ReferenceId = product.Id,
                Url = "/Seller/ProductsManager",
                IsRead = false,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return true;
        }
        //
        public async Task<bool> ConfirmOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return false;
            }

            if (order.Status != OrderStatus.Pending)
            {
                return false;
            }

            var oldStatus = order.Status;

            order.Status = OrderStatus.Confirmed;

            _context.OrderLogs.Add(new OrderLog
            {
                OrderId = order.Id,
                PreviousStatus = oldStatus.ToString(),
                Status = order.Status.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ActionBy = "Admin",
                ChangeType = "Confirm",
                Reason = "Admin xác nhận đơn hàng"
            });
            _context.Notifications.Add(new Notification
            {
                UserId = order.UserId,
                Title = "Đơn hàng đã được xác nhận",
                Content = $"Đơn hàng #{order.Id} đã được admin xác nhận.",
                Type = "Order",
                ReferenceId = order.Id,
                Url = $"/Oder/Details?orderId={order.Id}",
                IsRead = false,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return false;
            }

            if (order.Status == OrderStatus.Cancelled ||
                order.Status == OrderStatus.Completed)
            {
                return false;
            }

            var oldStatus = order.Status;

            order.Status = newStatus;

            _context.OrderLogs.Add(new OrderLog
            {
                OrderId = order.Id,
                PreviousStatus = oldStatus.ToString(),
                Status = newStatus.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ActionBy = "Admin",
                ChangeType = "UpdateStatus",
                Reason = "Admin cập nhật trạng thái đơn hàng"
            });
            _context.Notifications.Add(new Notification
            {
                UserId = order.UserId,
                Title = "Trạng thái đơn hàng đã thay đổi",
                Content = $"Đơn hàng #{order.Id} đã được cập nhật sang trạng thái {newStatus}.",
                Type = "Order",
                ReferenceId = order.Id,
                Url = $"/Oder/Details?orderId={order.Id}",
                IsRead = false,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return false;
            }

            if (order.Status == OrderStatus.Completed ||
                order.Status == OrderStatus.Cancelled)
            {
                return false;
            }

            var oldStatus = order.Status;

            order.Status = OrderStatus.Cancelled;

            _context.OrderLogs.Add(new OrderLog
            {
                OrderId = order.Id,
                PreviousStatus = oldStatus.ToString(),
                Status = order.Status.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ActionBy = "Admin",
                ChangeType = "Cancel",
                Reason = "Admin hủy đơn hàng"
            });
            _context.Notifications.Add(new Notification
            {
                UserId = order.UserId,
                Title = "Đơn hàng đã bị hủy",
                Content = $"Đơn hàng #{order.Id} đã bị admin hủy.",
                Type = "Order",
                ReferenceId = order.Id,
                Url = $"/Oder/Details?orderId={order.Id}",
                IsRead = false,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<FraudAnalysis?> GetOrderRiskAnalysisAsync(int orderId)
        {
            return await _context.FraudAnalyses
                .AsNoTracking()
                .Where(f => f.OrderId == orderId)
                .OrderByDescending(f => f.CreatedAt)
                .FirstOrDefaultAsync();
        }
        //
        public async Task<bool> AddCategoryAsync(CategoryManagementViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return false;
            }

            var category = new Models.Category
            {
                Name = model.Name.Trim(),

                // Form gửi 0 nghĩa là không có danh mục cha
                ParentId = model.ParentId == 0 ? null : model.ParentId,

                CreatedAt = DateTime.Now
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateCategoryAsync(CategoryManagementViewModel model)
        {
            if (model.Id == null || string.IsNullOrWhiteSpace(model.Name))
            {
                return false;
            }

            if (model.ParentId == model.Id)
            {
                return false;
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == model.Id);

            if (category == null)
            {
                return false;
            }

            category.Name = model.Name.Trim();

            // Form gửi 0 nghĩa là không có danh mục cha
            category.ParentId = model.ParentId == 0 ? null : model.ParentId;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.InverseParent)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                return false;
            }

            // Có sản phẩm hoặc danh mục con thì không xóa
            if (category.Products.Any() || category.InverseParent.Any())
            {
                return false;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<CategoryManagementViewModel?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return null;
            }

            var parentCategories = await _context.Categories
                .AsNoTracking()
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return new CategoryManagementViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ParentId = category.ParentId,
                ParentCategories = parentCategories
            };
        }
        public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false;
            }

            user.IsActive = isActive;
            _context.Notifications.Add(new Notification
            {
                UserId = user.Id,
                Title = isActive ? "Tài khoản đã được mở lại" : "Tài khoản đã bị khóa",
                Content = isActive
                    ? "Tài khoản của bạn đã được admin mở lại."
                    : "Tài khoản của bạn đã bị admin khóa.",
                Type = "Account",
                ReferenceId = user.Id,
                Url = "/",
                IsRead = false,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> ChangeUserToStaffAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return false;
            }

            var staffRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Code == "STAFF");

            if (staffRole == null)
            {
                return false;
            }

            // Xóa quyền cũ của user
            _context.UserRoles.RemoveRange(user.UserRoles);

            // Gán quyền STAFF
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = staffRole.Id
            };

            await _context.UserRoles.AddAsync(userRole);
            _context.Notifications.Add(new Notification
            {
                UserId = user.Id,
                Title = "Bạn đã được cấp quyền nhân viên",
                Content = "Tài khoản của bạn đã được admin chuyển thành nhân viên.",
                Type = "Account",
                ReferenceId = user.Id,
                Url = "/Admin/Dashboard",
                IsRead = false,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
