using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models.ViewModel.Category;
using Microsoft.EntityFrameworkCore;
namespace Demo_web_MVC.Repository.Category
{
    public class CategoryRepository: ICategoryRepository
    {
        public readonly AppDatabase _context;
        public CategoryRepository(AppDatabase context)
        {
            _context = context;
        }
        public async Task<List<Models.ViewModel.Category.CategoryViewModel>> GetAllAsyncCategory()
        {

            var categories = await _context.Categories
                                    .Where(c => c.Id != null)  // Handle NULL values
                                    .Select(c => new CategoryViewModel
                                    {
                                        Id = c.Id,
                                        Name = c.Name
                                    })
                                    .ToListAsync();

            return categories;
        }
    }
}
