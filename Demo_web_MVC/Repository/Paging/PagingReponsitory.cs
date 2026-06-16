using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_web_MVC.Repository.Paging
{
    
    public class PagingReponsitory:IPagingReponsitory
    {
        private readonly ILogger<PagingReponsitory> _logger;
        private readonly AppDatabase _context;

        public PagingReponsitory(ILogger<PagingReponsitory> logger, AppDatabase context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<PaginatedList<T>> GetPagedDataAsync<T>(IQueryable<T> query, int page, int pageSize)
        {
            // Kiểm tra nếu query là một IQueryable của EF
            if (!(query is IQueryable<T> efQuery))
            {
                throw new InvalidOperationException("The query must be an IQueryable from Entity Framework.");
            }

            var totalCount = await efQuery.CountAsync();  // Lấy tổng số bản ghi từ query

            var items = await efQuery
                .Skip((page - 1) * pageSize)  // Bỏ qua các bản ghi của các trang trước
                .Take(pageSize)  // Lấy số lượng bản ghi cho trang hiện tại
                .ToListAsync();  // Thực hiện truy vấn bất đồng bộ

            return new PaginatedList<T>(items, totalCount, page, pageSize);
        }
    }
}
