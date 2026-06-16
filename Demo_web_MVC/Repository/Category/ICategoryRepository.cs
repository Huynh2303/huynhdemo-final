namespace Demo_web_MVC.Repository.Category
{
    public interface ICategoryRepository
    {
        Task<List<Models.ViewModel.Category.CategoryViewModel>> GetAllAsyncCategory();
    }
}
