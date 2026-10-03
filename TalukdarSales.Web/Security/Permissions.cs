namespace TalukdarSales.Web.Security
{
    public record Permission(string Key, string Name, string Group);

    /// <summary>
    /// A request rule: the page path (and optionally a handler) and the permissions of which at least one is required.
    /// A handler-specific rule replaces the page-level rule for that handler.
    /// </summary>
    public record PathRule(string Path, string Handler, params string[] AnyOf);

    /// <summary>
    /// The fixed set of permissions. They are synced into the ApplicationModules table (Url = key) at startup,
    /// and roles are granted them through the Roles page.
    /// </summary>
    public static class Perm
    {
        public const string AdministratorRole = "Administrator";
        /// <summary>Rule marker: any signed-in user.</summary>
        public const string Anyone = "*";

        public const string Dashboard = "Dashboard";
        public const string Users = "Users";
        public const string Roles = "Roles";
        public const string RequisitionCreate = "Requisitions.Create";
        public const string RequisitionView = "Requisitions.View";
        public const string RequisitionApprove = "Requisitions.Approve";
        public const string InvoiceCreate = "Invoices.Create";
        public const string InvoiceView = "Invoices.View";
        public const string InvoiceCollect = "Invoices.Collect";
        public const string CollectionView = "Collections.View";
        public const string Production = "Production.View";
        public const string Notices = "Notices.Manage";
        public const string Reports = "Reports.View";
        public const string ProductTypes = "ProductTypes.Manage";
        public const string Products = "Products.Manage";
        public const string TimeSetting = "TimeSetting.Manage";

        public static readonly IReadOnlyList<Permission> Catalog = new List<Permission>
        {
            new(Dashboard, "View dashboard", "General"),

            new(Users, "Manage users", "User administration"),
            new(Roles, "Manage roles, permissions and user roles", "User administration"),

            new(RequisitionCreate, "Create requisitions", "Sales requisition"),
            new(RequisitionView, "View requisitions", "Sales requisition"),
            new(RequisitionApprove, "Approve requisitions (create invoices)", "Sales requisition"),

            new(InvoiceCreate, "Create manual invoices", "Sales invoice"),
            new(InvoiceView, "View, print and export invoices", "Sales invoice"),
            new(InvoiceCollect, "Collect payments", "Sales invoice"),
            new(CollectionView, "View collection history", "Sales invoice"),

            new(Production, "View production planning", "Operations"),
            new(Notices, "Manage notices", "Operations"),
            new(Reports, "View reports", "Operations"),

            new(ProductTypes, "Manage product types", "Settings"),
            new(Products, "Manage products", "Settings"),
            new(TimeSetting, "Change requisition time window", "Settings"),
        };

        /// <summary>Pages any signed-in user may open (no permission needed).</summary>
        public static readonly IReadOnlySet<string> OpenPages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/Login", "/Logout", "/AccessDenied", "/Error"
        };

        // Deny by default: a page with no rule here is forbidden to everyone except Administrators.
        public static readonly IReadOnlyList<PathRule> Rules = new List<PathRule>
        {
            // the notice feed is for everyone; managing notices needs a permission
            new("/Index", null, Anyone),
            new("/Index", "Save", Notices),
            new("/Index", "Remove", Notices),

            new("/Users/Index", null, Users),
            new("/Roles/Index", null, Roles),

            new("/Requisitions/Create", null, RequisitionCreate),
            new("/Requisitions/Index", null, RequisitionView),
            new("/Requisitions/Index", "Panel", RequisitionView),
            new("/Requisitions/Index", "Invoice", RequisitionApprove),
            new("/Requisitions/Index", "Bulk", RequisitionApprove),
            new("/Requisitions/Index", "Create", InvoiceCreate),

            new("/Invoices/Index", null, InvoiceView),
            new("/Invoices/Index", "Collect", InvoiceCollect),
            new("/Invoices/Details", null, InvoiceView),
            new("/Collections/Index", null, CollectionView),

            new("/Production/Index", null, Production),
            new("/Reports/Index", null, Reports),
            new("/Reports/Index", "Export", Reports),
            new("/Dashboard/Index", null, Dashboard),

            new("/ProductTypes/Index", null, ProductTypes),
            new("/Products/Index", null, Products),
            new("/TimeSetting/Index", null, TimeSetting),

            // dropdown helper used by the requisition, invoice and collection screens
            new("/Lookup", null, RequisitionCreate, RequisitionView, InvoiceCreate, InvoiceView, CollectionView),
        };

        public static Permission Find(string key) => Catalog.FirstOrDefault(p => p.Key == key);

        /// <summary>The permissions needed for a page/handler, or null when the page has no rule (denied).</summary>
        public static string[] Required(string page, string handler)
        {
            PathRule specific = null, general = null;
            foreach (var r in Rules)
            {
                if (!string.Equals(r.Path, page, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (r.Handler == null)
                    general = r;
                else if (string.Equals(r.Handler, handler, StringComparison.OrdinalIgnoreCase))
                    specific = r;
            }
            return (specific ?? general)?.AnyOf;
        }
    }
}
