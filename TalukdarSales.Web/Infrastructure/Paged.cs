namespace TalukdarSales.Web.Infrastructure
{
    public class Paged<T>
    {
        public List<T> Items { get; init; } = new();
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int Total { get; init; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);

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
