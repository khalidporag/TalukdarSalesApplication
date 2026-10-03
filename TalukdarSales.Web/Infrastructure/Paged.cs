namespace TalukdarSales.Web.Infrastructure
{
    public class Paged<T>
    {
        public List<T> Items { get; init; } = new();
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int Total { get; init; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);

        /// <summary>Pages in SQL: one COUNT plus one Skip/Take query. Prefer this for table-backed lists.</summary>
        public static Paged<T> Create(IQueryable<T> source, int page, int pageSize)
        {
            page = Math.Max(1, page);
            var total = source.Count();
            return new Paged<T>
            {
                Items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                Page = page,
                PageSize = pageSize,
                Total = total
            };
        }

        public static Paged<T> Create(IEnumerable<T> source, int page, int pageSize)
        {
            var list = source as IList<T> ?? source.ToList();
            page = Math.Max(1, page);
            return new Paged<T>
            {
                Items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                Page = page,
                PageSize = pageSize,
                Total = list.Count
            };
        }
    }
}
