using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Category;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Models.ViewModel.Search;
using Demo_web_MVC.Repository.Paging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace Demo_web_MVC.Repository.Search
{  
    public class SearchReponsitory:ISearchReponsitory
    {
        private readonly AppDatabase _context;
        private readonly ILogger<SearchReponsitory> _logger;
        private readonly IPagingReponsitory _pagingReponsitory;
        public SearchReponsitory(AppDatabase context, ILogger<SearchReponsitory> logger,IPagingReponsitory pagingReponsitory)
        {
            _context = context;
            _logger = logger;
           _pagingReponsitory = pagingReponsitory;
        }
        public async Task<SearchViewModel> SearchAsync(string searchQuery, int? page = null, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return new SearchViewModel
                {
                    ErrorMessage = "Vui lòng nhập từ khóa tìm kiếm.",
                    SearchStatus = "Error"
                };
            }

            searchQuery = searchQuery.Trim();

            var query = _context.Products
                .AsNoTracking()
                .Where(p =>
                    p.Name.Contains(searchQuery) ||
                    (p.Description != null && p.Description.Contains(searchQuery)) ||
                    (p.Category != null && p.Category.Name.Contains(searchQuery))
                )
                .OrderByDescending(p =>
                    p.Category != null && p.Category.Name.Contains(searchQuery)
                )
                .ThenByDescending(p => p.Name.Contains(searchQuery))
                .ThenBy(p => p.Name)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null,

                    Name = p.Name,
                    Description = p.Description,

                    imageUrl = p.ProductImages
                        .OrderBy(pi => pi.SortOrder)
                        .Select(pi => pi.Url)
                        .ToList(),

                    Variants = p.ProductVariants.Select(v => new ProductVariantsViewModel
                    {
                        Id = v.Id,
                        ProductId = v.ProductId,
                        Color = v.Color,
                        Size = v.Size,
                        Price = v.Price,
                        Stock = v.Stock,

                        ImageUrlsVariants = v.ProductVariantImages
                            .OrderBy(pvi => pvi.SortOrder)
                            .Select(pvi => pvi.Url)
                            .ToList()
                    }).ToList()
                });

            if (!page.HasValue)
            {
                var products = await query.ToListAsync();

                return new SearchViewModel
                {
                    SearchQuery = searchQuery,
                    ProductVMResults = products,
                    TotalResults = products.Count,
                    SearchStatus = "Success",
                    ErrorMessage = products.Any() ? null : "Không tìm thấy sản phẩm phù hợp."
                };
            }

            var paginatedProducts = await _pagingReponsitory
                .GetPagedDataAsync(query, page.Value, pageSize);

            return new SearchViewModel
            {
                SearchQuery = searchQuery,
                ProductVMResults = paginatedProducts.Items,
                TotalResults = paginatedProducts.TotalCount,
                SearchStatus = "Success",
                ErrorMessage = paginatedProducts.Items.Any() ? null : "Không tìm thấy sản phẩm phù hợp."
            };
        }

    }
}
