using Demo_web_MVC.Models.ViewModel.Search;
using Demo_web_MVC.Models;
namespace Demo_web_MVC.Service.Search
{
    public interface ISearchService
    {

        //Task<SearchViewModel> SearchAsync(string searchQuery);
        Task<SearchViewModel> SearchAsync(string searchQuery, int? page = null, int pageSize = 10);
    }
}
