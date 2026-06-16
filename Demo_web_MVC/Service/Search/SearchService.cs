using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Models.ViewModel.Search;
using Demo_web_MVC.Repository;
using Demo_web_MVC.Repository.Search;
using Newtonsoft.Json;
using System.Net.Http;

namespace Demo_web_MVC.Service.Search
{
    public class SearchService : ISearchService
    {
        private readonly ISearchReponsitory _searchReponsitory;
        private readonly ILogger<SearchService> _logger;
        private readonly IProductRepository _productRepository;


        // Tiêm IHttpClientFactory thay vì HttpClient trực tiếp
        public SearchService(ISearchReponsitory searchReponsitory,
            ILogger<SearchService> logger,
            IProductRepository productRepository
            ) // Tiêm IHttpClientFactory
        {
            _searchReponsitory = searchReponsitory;
            _logger = logger;
            _productRepository = productRepository;

        }
        //public async Task<SearchViewModel> SearchAsync(string searchQuery)
        //{
        //    if (string.IsNullOrWhiteSpace(searchQuery))
        //    {
        //        return new SearchViewModel
        //        {
        //            SearchQuery = searchQuery,
        //            ProductVMResults = new List<ProductViewModel>(),

        //            SearchStatus = "NoResults",
        //            ErrorMessage = "Please enter a search query."
        //        };
        //    }

        //    var result = await _searchReponsitory.SearchAsync(searchQuery);
        //    if (result == null || result.ProductVMResults == null || !result.ProductVMResults.Any())
        //    {
        //        result = new SearchViewModel
        //        {
        //            SearchQuery = searchQuery,
        //            ProductVMResults = new List<ProductViewModel>(),

        //            SearchStatus = "NoResults",
        //            ErrorMessage = $"No results found for '{searchQuery}'. Please try a different search term."
        //        };
        //    }

        //    return result;
        //}
        public async Task<SearchViewModel> SearchAsync(string searchQuery, int? page = null, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return new SearchViewModel
                {
                    SearchQuery = searchQuery,
                    ProductVMResults = new List<ProductViewModel>(),
                    TotalResults = 0,
                    SearchStatus = "Error",
                    ErrorMessage = "Please enter a valid search query."
                };
            }
            var result = await _searchReponsitory.SearchAsync(searchQuery, page, pageSize);
            if (result == null || result.ProductVMResults == null || !result.ProductVMResults.Any())
            {
                result = new SearchViewModel
                {
                    SearchQuery = searchQuery,
                    ProductVMResults = new List<ProductViewModel>(),
                    TotalResults = 0,
                    SearchStatus = "NoResults",
                    ErrorMessage = $"No results found for '{searchQuery}'. Please try a different search term."
                };
            }
            return result;
        }
    }
}
