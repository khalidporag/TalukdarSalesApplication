namespace TalukdarSales.Web.Infrastructure
{
    public class Paged<T>
    {
        public List<T> Items { get; init; } = new();
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int Total { get; init; }
        public PagerVm Pager(string url) => new(Page, PageSize, Total, url);
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);

        /// <summary>Pages in SQL: one COUNT plus one Skip/Take query. Prefer this for table-backed lists.</summary>
        public static Paged<T> Create(IQueryable<T> source, int page, int pageSize)
        {
            var total = source.Count();
            // a page past the end shows the last page instead of an empty list
            page = Math.Clamp(page, 1, Math.Max(1, pageSize <= 0 ? 1 : (int)Math.Ceiling(total / (double)pageSize)));
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
            page = Math.Clamp(page, 1, Math.Max(1, pageSize <= 0 ? 1 : (int)Math.Ceiling(list.Count / (double)pageSize)));
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

namespace TalukdarSales.Web.Infrastructure
{
    /// <summary>What the shared pager needs. <paramref name="Url"/> is the page address with its filters and no p= or size=.</summary>
    public record PagerVm(int Page, int PageSize, int Total, string Url)
    {
        public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
        public int From => Total == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int To => Math.Min(Total, Page * PageSize);
        public string Link(int page, int? size = null) =>
            Url + (Url.Contains('?') ? "&" : "?") + $"size={size ?? PageSize}&p={page}";
    }

    public static class PageSizes
    {
        public static readonly int[] Options = { 10, 25, 50, 100 };

        /// <summary>Only the offered sizes are accepted; anything else falls back to the page's default.</summary>
        public static int Clamp(int size, int fallback) => Options.Contains(size) ? size : fallback;
    }
}
