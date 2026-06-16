using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Product;

namespace Demo_web_MVC.Service
{
    public interface IProductService
    {
        Task < ProductViewModel> creat (ProductViewModel product, int sellerId);
        Task < ProductViewModel> update (int id,ProductViewModel product, int sellerId);
        Task < bool> delete (int id, int sellerId);
        
        Task < ProductViewModel> details (int id);        
        Task< PaginatedList<ProductViewModel>> getAll(int page, int pageSize);
        Task<ProductViewModel> getbyid(int id);
        Task<int?> GetProductIdByVariantIdAsync(int variantId);
        Task<ProductViewModel?> getbyidForSeller(int id, int sellerId);
        Task<PaginatedList<ProductViewModel>> GetProductsByCategoryAsync(int? categoryId, int page, int pageSize);
        Task<List<ProductViewModel>> GetRelatedProductsAsync();
    }                   
}
