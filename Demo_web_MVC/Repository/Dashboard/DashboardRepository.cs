using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel;
using Demo_web_MVC.Models.ViewModel.Address;
using Demo_web_MVC.Models.ViewModel.Dashboard;
using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Repository.Paging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static Demo_web_MVC.Models.ViewModel.Dashboard.DashboardViewModel;

namespace Demo_web_MVC.Repository.Dashboard
{
    public class DashboardRepository:IDashboardRepository
    {
        private readonly AppDatabase _context;
        private readonly  ILogger<DashboardRepository> _logger;
        private readonly IPagingReponsitory _pagingRepository;
        public DashboardRepository( AppDatabase context, ILogger<DashboardRepository> logger, IPagingReponsitory pagingRepository)
        {
            _context = context;
            _logger = logger;
            _pagingRepository = pagingRepository;
        }
        public async Task<DashboardViewModel> GetOrdersAndProductsAsync(
    int sellerId,
    int orderPage = 1,
    int productPage = 1,
    int pageSize = 5)
        {
            try
            {
                _logger.LogInformation("Bắt đầu lấy dashboard seller {SellerId}", sellerId);

                var ordersQuery = _context.Orders
                    .AsNoTracking()
                    .Where(o => o.OrderItems.Any(item =>
                        item.Variant.Product.SellerId == sellerId))
                    .OrderByDescending(o => o.Id)
                    
                    .Select(o => new OderViewModel
                    {
                        Id = o.Id,

                        TotalAmount = o.OrderItems
                            .Where(item => item.Variant.Product.SellerId == sellerId)
                            .Sum(item => item.Price * item.Quantity),

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

                        Items = o.OrderItems
                            .Where(item => item.Variant.Product.SellerId == sellerId)
                            .Select(item => new OderItemViewModel
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
                            })
                            .ToList()
                    });

                var productsQuery = _context.Products
                    .AsNoTracking()
                    .Where(p => p.SellerId == sellerId && !p.IsDeleted)
                    .OrderByDescending(p => p.Id)
                    .Select(p => new ProductViewModel
                    {
                        Id = p.Id,
                        CategoryId = p.CategoryId,
                        Name = p.Name,

                        imageUrl = p.ProductImages
                            .Select(pi => pi.Url)
                            .ToList(),

                        Variants = p.ProductVariants
                            .Select(v => new ProductVariantsViewModel
                            {
                                Price = v.Price,
                                Stock = v.Stock,
                            })
                            .ToList()
                    });

                var orders = await _pagingRepository.GetPagedDataAsync(
                    ordersQuery,
                    orderPage,
                    pageSize
                );

                var products = await _pagingRepository.GetPagedDataAsync(
                    productsQuery,
                    productPage,
                    pageSize
                );

                return new DashboardViewModel
                {
                    Orders = orders,
                    Products = products
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dashboard seller {SellerId}", sellerId);
                throw;
            }
        }
        public async Task<ProductsManagerViewModel> GetProductsManagerAsync(
    int sellerId,
    int page = 1,
    int pageSize = 10)
        {
            var productsQuery = _context.Products
                .AsNoTracking()
                .Where(p => p.SellerId == sellerId && !p.IsDeleted)
                .OrderByDescending(p => p.Id)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    Name = p.Name,

                    imageUrl = p.ProductImages
                        .Select(pi => pi.Url)
                        .ToList(),

                    Variants = p.ProductVariants
                        .Select(v => new ProductVariantsViewModel
                        {
                            Price = v.Price,
                            Stock = v.Stock,
                        })
                        .ToList()
                });

            var pagedProducts = await _pagingRepository
                .GetPagedDataAsync(productsQuery, page, pageSize);

            return new ProductsManagerViewModel
            {
                Products = pagedProducts
            };
        }
        public async Task<List<DetailsOrderDashboardViewmodel>> GetDetailsOrderDashboardViewmodelAsync(int orderId,int sellerId)
        {
            var order = await _context.Orders
                .Where(o => o.Id == orderId)
                .Where(o => o.OrderItems.Any(oi =>
                    oi.Variant.Product.SellerId == sellerId))
                .Select(o => new DetailsOrderDashboardViewmodel
                {
                    OrderId = o.Id,
                    Email = o.User.Email,
                    OrderStatus = o.Status.ToString(),
                    TotalAmount = o.OrderItems
                        .Where(oi => oi.Variant.Product.SellerId == sellerId)
                        .Sum(oi => oi.Price * oi.Quantity),
                    CreatedAt = o.CreatedAt,

                    AddressView = o.User.Addresses
                        .Select(a => new AddressViewModel
                        {
                            RecipientName = a.RecipientName,
                            PhoneNumber = a.PhoneNumber,
                            AddressLine = a.AddressLine,
                            City = a.City,
                            Country = a.Country
                        })
                        .FirstOrDefault(),

                    OderItemViews = o.OrderItems
                        .Where(oi => oi.Variant.Product.SellerId == sellerId)
                        .Select(oi => new OderItemViewModel
                        {
                            OrderId = oi.Id,
                            Name = oi.Variant.Product.Name,
                            Price = oi.Price,
                            Quantity = oi.Quantity,

                            Img = oi.Variant.ProductVariantImages
                                .OrderBy(img => img.SortOrder)
                                .Select(img => img.Url)
                                .FirstOrDefault()
                                ?? oi.Variant.Product.ProductImages
                                    .Select(img => img.Url)
                                    .FirstOrDefault()
                                ?? "/uploads/images/no-image.jpg"
                        })
                        .ToList(),

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
                        .FirstOrDefault()
                })
                .ToListAsync();

            if (!order.Any())
            {
                _logger.LogError("Seller {SellerId} không có quyền xem OrderId: {OrderId}", sellerId, orderId);
                return new List<DetailsOrderDashboardViewmodel>();
            }

            foreach (var item in order)
            {
                if (item.FraudAnalysis != null &&
                    !string.IsNullOrEmpty(item.FraudAnalysis.RiskReasons))
                {
                    item.FraudAnalysis.Reasons =
                        JsonSerializer.Deserialize<List<string>>(item.FraudAnalysis.RiskReasons)
                        ?? new List<string>();
                }
            }

            return order;
        }
        public async Task<StatisticsViewModel> GetDashboardStatisticsAsync(int sellerId)
        {
            var today = DateTime.Today;

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .Where(o => o.CreatedAt.Date == today)
                .Where(o => o.OrderItems.Any(oi =>
                    oi.Variant.Product.SellerId == sellerId))
                .ToListAsync();

            var statistics = new StatisticsViewModel();

            statistics.TotalOrders = orders.Count;

            statistics.TotalProducts = orders.Sum(o => o.OrderItems
                .Where(i => i.Variant.Product.SellerId == sellerId)
                .Sum(i => i.Quantity));

            statistics.TotalRevenue = orders.Sum(o => o.OrderItems
                .Where(i => i.Variant.Product.SellerId == sellerId)
                .Sum(i => i.Price * i.Quantity));

            statistics.Orders = orders
                .Select(o => new OderViewModel
                {
                    CreateAt = o.CreatedAt,
                    TotalAmount = o.OrderItems
                        .Where(i => i.Variant.Product.SellerId == sellerId)
                        .Sum(i => i.Price * i.Quantity)
                })
                .OrderBy(o => o.CreateAt)
                .ToList();

            var last7Orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .Where(o => o.CreatedAt >= today.AddDays(-6))
                .Where(o => o.OrderItems.Any(oi =>
                    oi.Variant.Product.SellerId == sellerId))
                .ToListAsync();

            statistics.RevenueLast7Days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-6 + i))
                .ToDictionary(
                    d => d,
                    d => last7Orders
                        .Where(o => o.CreatedAt.Date == d)
                        .Sum(o => o.OrderItems
                            .Where(i => i.Variant.Product.SellerId == sellerId)
                            .Sum(i => i.Price * i.Quantity))
                );

            var last30Orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .Where(o => o.CreatedAt >= today.AddDays(-29))
                .Where(o => o.OrderItems.Any(oi =>
                    oi.Variant.Product.SellerId == sellerId))
                .ToListAsync();

            statistics.RevenueLast30Days = Enumerable.Range(0, 30)
                .Select(i => today.AddDays(-29 + i))
                .ToDictionary(
                    d => d,
                    d => last30Orders
                        .Where(o => o.CreatedAt.Date == d)
                        .Sum(o => o.OrderItems
                            .Where(i => i.Variant.Product.SellerId == sellerId)
                            .Sum(i => i.Price * i.Quantity))
                );

            var allOrders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product)
                .Where(o => o.OrderItems.Any(oi =>
                    oi.Variant.Product.SellerId == sellerId))
                .ToListAsync();

            statistics.OrderStatusAll = allOrders
                .GroupBy(o => o.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var last7Days = today.AddDays(-6);

            statistics.OrderStatusLast7Days = Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .ToDictionary(
                    s => s,
                    s => allOrders.Count(o =>
                        o.Status == s &&
                        o.CreatedAt.Date >= last7Days)
                );

            var last30Days = today.AddDays(-29);

            statistics.OrderStatusLast30Days = Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .ToDictionary(
                    s => s,
                    s => allOrders.Count(o =>
                        o.Status == s &&
                        o.CreatedAt.Date >= last30Days)
                );

            return statistics;
        }
    }
}
