using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Product;
using NuGet.DependencyResolver;

namespace Demo_web_MVC.Repository
{
    public interface IProductRepository
    {
        Task<PaginatedList<Models.ViewModel.Product.ProductViewModel>> GetAllAsync(int page, int pageSize);
                              
        Task<Models.ViewModel.Product.ProductViewModel> GetByIdAsync(int id);
        Task<ProductViewModel> AddAsnyc (ProductViewModel product, int sellerId);
        Task<ProductViewModel> UpdateAsync(int id,ProductViewModel product, int sellerId);
        Task<bool> DeleteAsync(int id, int sellerId);
        Task<ProductViewModel> DetailsAsnyc(int id);
        Task<int?> GetProductIdByVariantIdAsync(int variantId);
        Task<ProductViewModel?> GetByIdForSellerAsync(int id, int sellerId);
        Task<PaginatedList<ProductViewModel>> GetProductsByCategoryAsync(int? categoryId,int page,int pageSize);
        Task<List<ProductViewModel>> GetRelatedProductsAsync();
    }
}
