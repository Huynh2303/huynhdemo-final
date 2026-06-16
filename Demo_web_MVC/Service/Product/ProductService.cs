using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Repository;
using Demo_web_MVC.Repository.Product;
namespace Demo_web_MVC.Service.Product
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;
        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }       
        public async Task<ProductViewModel> details(int id)    
        {
            
            return await _productRepository.DetailsAsnyc(id);
        }
        public async Task<ProductViewModel> creat(
        ProductViewModel product,
        int sellerId)
        {
            try
            {
                if (product == null ||
                    string.IsNullOrEmpty(product.Name) ||
                    product.CategoryId <= 0)
                {
                    throw new ArgumentException("Thông tin sản phẩm không hợp lệ.");
                }

                return await _productRepository
                    .AddAsnyc(product, sellerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo sản phẩm");

                throw new Exception(
                    $"Có lỗi khi tạo sản phẩm: {ex.Message}",
                    ex);
            }
        }
        public async Task<ProductViewModel> update(
        int id,
        ProductViewModel product,
        int sellerId)
        {
            return await _productRepository
                .UpdateAsync(id, product, sellerId);
        }
        public async Task<bool> delete(int id, int sellerId)
        {
            if (id <= 0)
            {
                _logger.LogWarning("ProductId không hợp lệ: {ProductId}", id);
                return false;
            }

            if (sellerId <= 0)
            {
                _logger.LogWarning("SellerId không hợp lệ: {SellerId}", sellerId);
                return false;
            }

            return await _productRepository
                .DeleteAsync(id, sellerId);
        }
        public async Task<PaginatedList<ProductViewModel>> getAll(int page, int pageSize)
        {
            return await _productRepository.GetAllAsync(page,pageSize);
        }
        public async Task<ProductViewModel> getbyid(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }
        public async Task<int?> GetProductIdByVariantIdAsync(int variantId)
        {
            return await _productRepository.GetProductIdByVariantIdAsync(variantId);
        }
        public async Task<ProductViewModel?> getbyidForSeller(int id,int sellerId)
        {
            return await _productRepository
                .GetByIdForSellerAsync(id, sellerId);
        }
        public async Task<PaginatedList<ProductViewModel>> GetProductsByCategoryAsync(
            int? categoryId,
            int page,
            int pageSize)
        {
            return await _productRepository.GetProductsByCategoryAsync(
                categoryId,
                page,
                pageSize);
        }
        public async Task<List<ProductViewModel>> GetRelatedProductsAsync()
        {
            return await _productRepository.GetRelatedProductsAsync();
        }
    }
}