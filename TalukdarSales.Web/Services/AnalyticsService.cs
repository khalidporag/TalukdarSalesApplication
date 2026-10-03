using TalukdarSales.Web.Interfaces;

namespace TalukdarSales.Web.Services
{
    /// <summary>A reporting window plus the equally long window right before it.</summary>
    public record Period(string Key, string Label, DateTime Start, DateTime EndExclusive)
    {
        public int Days => (int)(EndExclusive - Start).TotalDays;
        public DateTime PrevStart => Start.AddDays(-Days);
        public DateTime PrevEnd => Start;

        public static readonly (string Key, string Label)[] Choices =
        {
            ("today", "Today"), ("7", "7 days"), ("30", "30 days"), ("month", "This month"), ("90", "Quarter"),
        };

        public static Period Parse(string key, DateTime? now = null)
        {
            var today = (now ?? DateTime.Now).Date;
            return key switch
            {
                "today" => new Period("today", "today", today, today.AddDays(1)),
                "7" => new Period("7", "last 7 days", today.AddDays(-6), today.AddDays(1)),
                "month" => new Period("month", "this month", new DateTime(today.Year, today.Month, 1), today.AddDays(1)),
                "90" => new Period("90", "last 90 days", today.AddDays(-89), today.AddDays(1)),
                _ => new Period("30", "last 30 days", today.AddDays(-29), today.AddDays(1)),
            };
        }
    }

    public record DayPoint(DateTime Date, double Billed, double Collected);
    public record NameValue(string Name, double Value);
    public record CategoryShare(string Name, double Amount, double PrevAmount);
    public record AgeBucket(string Name, double Amount);
    public record ProductRank(string Name, string Category, double Quantity, double PrevQuantity, double[] Spark);
    public record CreditAlert(int UserId, string Name, double Due, double Limit);
    public record StaffSales(int UserId, string Name, int Orders, double Billed, double Collected);
    public record WeekPoint(DateTime Start, double Billed, double Collected);

    public class Overview
    {
        public Period Period { get; init; }
        public double Billed, PrevBilled, Collected, PrevCollected, Outstanding;
        public int OutstandingCustomers, Orders, PrevOrders, ActiveCustomers, PrevActiveCustomers, TotalCustomers;
        public int Over30Count; public double Over30Amount;
        public int Waiting; public double WaitingAmount; public DateTime? OldestWaiting;
        public double CollectionRate => Billed <= 0 ? 0 : Collected / Billed * 100;
        public double PrevCollectionRate => PrevBilled <= 0 ? 0 : PrevCollected / PrevBilled * 100;
        public double AverageOrder => Orders == 0 ? 0 : Billed / Orders;
        public double PrevAverageOrder => PrevOrders == 0 ? 0 : PrevBilled / PrevOrders;
        public List<DayPoint> Days { get; init; } = new();
        public List<double> PrevBilledDays { get; init; } = new();
        public List<WeekPoint> Weeks { get; init; } = new();
        public List<NameValue> TopCustomers { get; init; } = new();
        public List<CategoryShare> Categories { get; init; } = new();
        public List<AgeBucket> Aging { get; init; } = new();
        public List<ProductRank> Products { get; init; } = new();
        public List<int> OrdersByWeekday { get; init; } = new();   // Sun..Sat
        public List<CreditAlert> CreditAlerts { get; init; } = new();
        public List<StaffSales> Staff { get; init; } = new();
    }

    public class AnalyticsService
    {
        private readonly ISalesInvoiceRepository _invoices;
        private readonly ISalesInvoiceDetailsRepository _invoiceDetails;
        private readonly ISalesRequisitionRepository _requisitions;
        private readonly ISalesRequisitionDetailRepository _requisitionDetails;
        private readonly ICollectionLedgerRepository _ledger;
        private readonly IFinishedGoodsRepository _goods;
        private readonly IFinishedGoodTypeRepository _types;
        private readonly IUserRepository _users;

        public AnalyticsService(ISalesInvoiceRepository invoices, ISalesInvoiceDetailsRepository invoiceDetails,
            ISalesRequisitionRepository requisitions, ISalesRequisitionDetailRepository requisitionDetails,
            ICollectionLedgerRepository ledger, IFinishedGoodsRepository goods, IFinishedGoodTypeRepository types,
            IUserRepository users)
        {
            _invoices = invoices; _invoiceDetails = invoiceDetails; _requisitions = requisitions;
            _requisitionDetails = requisitionDetails; _ledger = ledger; _goods = goods; _types = types; _users = users;
        }

        private double Billed(DateTime from, DateTime to) =>
            _invoices.GetAll().Where(i => i.CreatedDateTime >= from && i.CreatedDateTime < to).Sum(i => (double?)i.TotalPrice) ?? 0;

        private double Collected(DateTime from, DateTime to) =>
            _ledger.GetAll().Where(c => c.CreatedOn >= from && c.CreatedOn < to).Sum(c => (double?)c.CollectionAmount) ?? 0;

        private Dictionary<DateTime, double> BilledByDay(DateTime from, DateTime to) =>
            _invoices.GetAll().Where(i => i.CreatedDateTime >= from && i.CreatedDateTime < to)
                .GroupBy(i => i.CreatedDateTime.Date).Select(g => new { Day = g.Key, V = g.Sum(i => i.TotalPrice) })
                .ToList().ToDictionary(x => x.Day, x => x.V);

        private Dictionary<DateTime, double> CollectedByDay(DateTime from, DateTime to) =>
            _ledger.GetAll().Where(c => c.CreatedOn >= from && c.CreatedOn < to)
                .GroupBy(c => c.CreatedOn.Date).Select(g => new { Day = g.Key, V = g.Sum(c => c.CollectionAmount) })
                .ToList().ToDictionary(x => x.Day, x => x.V);

        public double MonthToDateBilled() =>
            Billed(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), DateTime.Now.Date.AddDays(1));

        public Overview Build(Period p)
        {
            var today = DateTime.Now.Date;
            // Charts need a few points: a single-day period still draws the last 14 days.
            var chartStart = p.Days < 7 ? p.EndExclusive.AddDays(-14) : p.Start;
            var chartDays = (int)(p.EndExclusive - chartStart).TotalDays;

            var billedDay = BilledByDay(chartStart.AddDays(-chartDays), p.EndExclusive);
            var collDay = CollectedByDay(chartStart, p.EndExclusive);
            double V(Dictionary<DateTime, double> d, DateTime k) => d.TryGetValue(k, out var v) ? v : 0;
            var days = Enumerable.Range(0, chartDays).Select(i => chartStart.AddDays(i))
                .Select(d => new DayPoint(d, V(billedDay, d), V(collDay, d))).ToList();
            var prevDays = Enumerable.Range(0, chartDays).Select(i => V(billedDay, chartStart.AddDays(i - chartDays))).ToList();

            // Weekly buckets ending at the period end, newest last.
            var weekCount = Math.Clamp((int)Math.Ceiling(p.Days / 7.0), 4, 13);
            var wStart = p.EndExclusive.AddDays(-7 * weekCount);
            var wBilled = BilledByDay(wStart, p.EndExclusive);
            var wColl = CollectedByDay(wStart, p.EndExclusive);
            var weeks = Enumerable.Range(0, weekCount).Select(w =>
            {
                var s = wStart.AddDays(7 * w);
                var range = Enumerable.Range(0, 7).Select(i => s.AddDays(i)).ToList();
                return new WeekPoint(s, range.Sum(d => V(wBilled, d)), range.Sum(d => V(wColl, d)));
            }).ToList();

            var customerIds = _requisitions.GetAll().Where(r => !r.IsCancelled && r.CreatedDateTime >= p.Start && r.CreatedDateTime < p.EndExclusive)
                .Select(r => r.UserId).Distinct().Count();
            var prevCustomerIds = _requisitions.GetAll().Where(r => !r.IsCancelled && r.CreatedDateTime >= p.PrevStart && r.CreatedDateTime < p.PrevEnd)
                .Select(r => r.UserId).Distinct().Count();

            var weekday = _requisitions.GetAll().Where(r => !r.IsCancelled && r.CreatedDateTime >= p.Start && r.CreatedDateTime < p.EndExclusive)
                .Select(r => r.CreatedDateTime).ToList()
                .GroupBy(d => (int)d.DayOfWeek).ToDictionary(g => g.Key, g => g.Count());

            var waiting = _requisitions.GetAll().Where(r => r.IsActive).Select(r => new { r.Id, r.CreatedDateTime }).ToList();
            var waitIds = waiting.Select(w => w.Id).ToList();
            var waitAmount = waitIds.Count == 0 ? 0 :
                _requisitionDetails.GetAll().Where(d => waitIds.Contains(d.SalesRequisitionId)).Sum(d => (double?)(d.Quantity * d.Price)) ?? 0;

            var o = new Overview
            {
                Period = p, Days = days, PrevBilledDays = prevDays, Weeks = weeks,
                Billed = Billed(p.Start, p.EndExclusive), PrevBilled = Billed(p.PrevStart, p.PrevEnd),
                Collected = Collected(p.Start, p.EndExclusive), PrevCollected = Collected(p.PrevStart, p.PrevEnd),
                Orders = _requisitions.GetAll().Count(r => !r.IsCancelled && r.CreatedDateTime >= p.Start && r.CreatedDateTime < p.EndExclusive),
                PrevOrders = _requisitions.GetAll().Count(r => !r.IsCancelled && r.CreatedDateTime >= p.PrevStart && r.CreatedDateTime < p.PrevEnd),
                ActiveCustomers = customerIds, PrevActiveCustomers = prevCustomerIds,
                TotalCustomers = _users.GetAll().Count(),
                Waiting = waiting.Count, WaitingAmount = waitAmount,
                OldestWaiting = waiting.Count == 0 ? null : waiting.Min(w => w.CreatedDateTime),
                OrdersByWeekday = Enumerable.Range(0, 7).Select(i => weekday.TryGetValue(i, out var c) ? c : 0).ToList(),
            };

            LoadOutstanding(o, today);
            o.TopCustomers.AddRange(TopCustomers(p));
            o.Categories.AddRange(Categories(p));
            o.Products.AddRange(Products(p, today));
            o.CreditAlerts.AddRange(CreditAlerts());
            o.Staff.AddRange(StaffBoard(p));
            return o;
        }

        private void LoadOutstanding(Overview o, DateTime today)
        {
            var unpaid = _invoices.GetAll().Where(i => i.TotalPrice - i.CollectionAmount > 0.005)
                .Select(i => new { i.UserId, i.CreatedDateTime, Due = i.TotalPrice - i.CollectionAmount }).ToList();
            var credit = _users.GetAll().Select(u => new { u.Id, u.MaxCreditDays }).ToList().ToDictionary(u => u.Id, u => u.MaxCreditDays);
            var buckets = new double[4];
            foreach (var i in unpaid)
            {
                var late = (today - i.CreatedDateTime.Date).Days - (credit.TryGetValue(i.UserId, out var d) ? d : 0);
                var b = late <= 0 ? 0 : late <= 15 ? 1 : late <= 30 ? 2 : 3;
                buckets[b] += i.Due;
                if (b == 3) { o.Over30Amount += i.Due; o.Over30Count++; }
            }
            o.Outstanding = unpaid.Sum(i => i.Due);
            o.OutstandingCustomers = unpaid.Select(i => i.UserId).Distinct().Count();
            o.Aging.AddRange(new[] { "Not yet due", "1 to 15 days late", "16 to 30 days late", "Over 30 days late" }
                .Select((n, i) => new AgeBucket(n, buckets[i])));
        }

        private List<NameValue> TopCustomers(Period p)
        {
            var top = _invoices.GetAll().Where(i => i.CreatedDateTime >= p.Start && i.CreatedDateTime < p.EndExclusive)
                .GroupBy(i => i.UserId).Select(g => new { UserId = g.Key, Amount = g.Sum(i => i.TotalPrice) })
                .OrderByDescending(x => x.Amount).Take(5).ToList();
            var ids = top.Select(t => t.UserId).ToList();
            var users = _users.GetAll().Where(u => ids.Contains(u.Id)).ToDictionary(u => u.Id);
            return top.Select(t => new NameValue(users.TryGetValue(t.UserId, out var u) ? $"{u.FirstName} {u.LastName}".Trim() : "", t.Amount)).ToList();
        }

        private Dictionary<int, double> SalesByGood(DateTime from, DateTime to) =>
            _invoiceDetails.GetAll().Where(d => d.CreatedDateTime >= from && d.CreatedDateTime < to)
                .GroupBy(d => d.FinishedGoodsId).Select(g => new { Id = g.Key, V = g.Sum(d => d.Quantity * d.Price) })
                .ToList().ToDictionary(x => x.Id, x => x.V);

        private List<CategoryShare> Categories(Period p)
        {
            var cur = SalesByGood(p.Start, p.EndExclusive);
            var prev = SalesByGood(p.PrevStart, p.PrevEnd);
            var goodType = _goods.GetAll().Select(g => new { g.Id, g.GoodTypeId }).ToList().ToDictionary(g => g.Id, g => g.GoodTypeId);
            var typeName = _types.GetAll().Select(t => new { t.Id, t.Name }).ToList().ToDictionary(t => t.Id, t => t.Name);
            string Name(int goodId) => goodType.TryGetValue(goodId, out var t) && typeName.TryGetValue(t, out var n) ? n : "Other";
            var names = cur.Keys.Concat(prev.Keys).Select(Name).Distinct().ToList();
            return names.Select(n => new CategoryShare(n,
                    cur.Where(k => Name(k.Key) == n).Sum(k => k.Value), prev.Where(k => Name(k.Key) == n).Sum(k => k.Value)))
                .Where(c => c.Amount > 0 || c.PrevAmount > 0).OrderByDescending(c => c.Amount).ToList();
        }

        private List<ProductRank> Products(Period p, DateTime today)
        {
            var qty = _invoiceDetails.GetAll().Where(d => d.CreatedDateTime >= p.Start && d.CreatedDateTime < p.EndExclusive)
                .GroupBy(d => d.FinishedGoodsId).Select(g => new { Id = g.Key, Q = g.Sum(d => d.Quantity) }).ToList();
            var prev = _invoiceDetails.GetAll().Where(d => d.CreatedDateTime >= p.PrevStart && d.CreatedDateTime < p.PrevEnd)
                .GroupBy(d => d.FinishedGoodsId).Select(g => new { Id = g.Key, Q = g.Sum(d => d.Quantity) }).ToList().ToDictionary(x => x.Id, x => x.Q);
            var sparkStart = today.AddDays(-6);
            var spark = _invoiceDetails.GetAll().Where(d => d.CreatedDateTime >= sparkStart && d.CreatedDateTime < today.AddDays(1))
                .GroupBy(d => new { d.FinishedGoodsId, Day = d.CreatedDateTime.Date })
                .Select(g => new { g.Key.FinishedGoodsId, g.Key.Day, Q = g.Sum(d => d.Quantity) }).ToList();
            var goods = _goods.GetAll().Select(g => new { g.Id, g.Name, g.GoodTypeId }).ToList().ToDictionary(g => g.Id);
            var typeName = _types.GetAll().Select(t => new { t.Id, t.Name }).ToList().ToDictionary(t => t.Id, t => t.Name);
            return qty.Where(x => goods.ContainsKey(x.Id)).Select(x =>
            {
                var g = goods[x.Id];
                var s = Enumerable.Range(0, 7).Select(i => spark.Where(r => r.FinishedGoodsId == x.Id && r.Day == sparkStart.AddDays(i)).Sum(r => r.Q)).ToArray();
                return new ProductRank(g.Name, typeName.TryGetValue(g.GoodTypeId, out var n) ? n : "", x.Q, prev.TryGetValue(x.Id, out var pq) ? pq : 0, s);
            }).OrderByDescending(x => x.Quantity).ToList();
        }

        /// <summary>Per salesperson: orders taken, and the billed and collected value of the invoices those orders became.</summary>
        private List<StaffSales> StaffBoard(Period p)
        {
            var orders = _requisitions.GetAll().Where(r => !r.IsCancelled && r.CreatedDateTime >= p.Start && r.CreatedDateTime < p.EndExclusive)
                .GroupBy(r => r.CreatedByUserId).Select(g => new { UserId = g.Key, N = g.Count() }).ToList().ToDictionary(x => x.UserId, x => x.N);
            var sales = (from i in _invoices.GetAll().Where(i => i.CreatedDateTime >= p.Start && i.CreatedDateTime < p.EndExclusive)
                         join r in _requisitions.GetAll() on i.SalesRequisitionId equals r.Id
                         select new { r.CreatedByUserId, i.TotalPrice, i.CollectionAmount }).ToList()
                .GroupBy(x => x.CreatedByUserId).ToDictionary(g => g.Key, g => (Billed: g.Sum(x => x.TotalPrice), Paid: g.Sum(x => x.CollectionAmount)));
            var ids = orders.Keys.Concat(sales.Keys).Distinct().ToList();
            var names = _users.GetAll().Where(u => ids.Contains(u.Id)).ToList().ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim());
            return ids.Select(id => new StaffSales(id, id == 0 ? "Not recorded" : names.TryGetValue(id, out var n) ? n : "Former staff",
                    orders.TryGetValue(id, out var c) ? c : 0, sales.TryGetValue(id, out var s) ? s.Billed : 0, sales.TryGetValue(id, out var s2) ? s2.Paid : 0))
                .OrderByDescending(x => x.Billed).ToList();
        }

        private List<CreditAlert> CreditAlerts() =>
            _users.GetAll().Where(u => u.MaxCreditLimit > 0 && u.DueAmount >= u.MaxCreditLimit)
                .OrderByDescending(u => u.DueAmount).Take(10).ToList()
                .Select(u => new CreditAlert(u.Id, $"{u.FirstName} {u.LastName}".Trim(), u.DueAmount, u.MaxCreditLimit)).ToList();
    }
}
