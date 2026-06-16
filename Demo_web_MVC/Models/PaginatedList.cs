namespace Demo_web_MVC.Models
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; }

        public int PageIndex { get; set; }

        public int TotalPages { get; set; }

        public int TotalCount { get; set; }

        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex < TotalPages;

        public PaginatedList(
            List<T> items,
            int totalCount,
            int pageIndex,
            int pageSize)
        {
            Items = items;

            TotalCount = totalCount;

            PageIndex = pageIndex;

            TotalPages = (int)Math.Ceiling(
                totalCount / (double)pageSize);
        }
    }
}
