using Demo_web_MVC.Models.ViewModel.Category;

namespace Demo_web_MVC.Service.Category
{
    public interface ICategoryService
    {
        Task<List<CategoryViewModel>> GetAllCategories();
    }
}
