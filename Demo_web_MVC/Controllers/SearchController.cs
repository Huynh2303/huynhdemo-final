using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Models.ViewModel.Search;
using Demo_web_MVC.Service.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Demo_web_MVC.Controllers
{
    [AllowAnonymous]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;
        public SearchController(ISearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        

        public async Task<IActionResult> Index(string searchQuery, int? page = 1)
        {
            _logger.LogInformation("Start search operation with query: {SearchQuery} and page: {Page}", searchQuery, page);

            int pageSize = 20;  // Số sản phẩm mỗi trang
            var result = await _searchService.SearchAsync(searchQuery, page, pageSize);

            // Tạo đối tượng PagingInfo và gán thông tin phân trang
            result.PagingInfo = new PagingInfo
            {
                CurrentPage = page ?? 1,
                ItemsPerPage = pageSize,
                TotalItems = result.TotalResults,
                TotalPages = (int)Math.Ceiling((decimal)result.TotalResults / pageSize)
            };

            _logger.LogInformation("Found {TotalResults} results for query: {SearchQuery} on page: {Page}", result.TotalResults, searchQuery, page);

            // Trả về view với kết quả tìm kiếm
            return View(result);
        }

    }
}
