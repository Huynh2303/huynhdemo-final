using Demo_web_MVC.Models.ViewModel.Category;

namespace Demo_web_MVC.Service.Category
{
    public class CategoryService:ICategoryService
    {
        public readonly Repository.Category.ICategoryRepository _categoryRepository;
        public CategoryService(Repository.Category.ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<List<CategoryViewModel>> GetAllCategories()
        {
            return await _categoryRepository.GetAllAsyncCategory();
        }
    }
}
