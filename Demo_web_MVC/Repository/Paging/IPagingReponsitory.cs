using Demo_web_MVC.Models;

namespace Demo_web_MVC.Repository.Paging
{
    public interface IPagingReponsitory
    {
        Task<PaginatedList<T>> GetPagedDataAsync<T>(IQueryable<T> query, int page , int pageSize);
    }
}
