using System.Net;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TalukdarSales.Web.Context;
using TalukdarSales.Web.Security;
using TalukdarSales.Web.Services;
using Xunit;

namespace TalukdarSales.Tests
{
    public class RbacTests
    {
        private static async Task<HttpClient> LoginAs(TestApp app, string user)
        {
            var c = app.NewClient();
            var res = await TestApp.Login(c, user, "pw");
            Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
            return c;
        }

        private static (TestApp app, int adminId) NewApp()
        {
            var app = new TestApp();
            var admin = app.SeedUser("admin", "pw", "Ada");
            return (app, admin);
        }

        private static bool Denied(HttpResponseMessage r) =>
            r.StatusCode == HttpStatusCode.Forbidden ||
            (r.StatusCode == HttpStatusCode.Redirect && r.Headers.Location!.OriginalString.Contains("/AccessDenied"));

        // ---- rule table -------------------------------------------------------------------------------------

        private static List<string> PagePaths(TestApp app) =>
            app.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
                .Select(e => e.Metadata.GetMetadata<PageActionDescriptor>()).Where(d => d != null)
                .Select(d => d.ViewEnginePath).Distinct().ToList();

        [Fact]
        public void Every_page_is_either_open_or_has_a_rule_and_every_rule_points_at_a_real_page()
        {
            var app = new TestApp();
            var pages = PagePaths(app);
            Assert.NotEmpty(pages);
            foreach (var page in pages.Where(p => !Perm.OpenPages.Contains(p)))
                Assert.True(Perm.Required(page, null) != null, $"page {page} has no permission rule (it would be admin-only)");
            foreach (var rule in Perm.Rules)
                Assert.Contains(rule.Path, pages);
            foreach (var key in Perm.Rules.SelectMany(r => r.AnyOf).Where(k => k != Perm.Anyone))
                Assert.NotNull(Perm.Find(key));
        }

        [Fact]
        public void Handler_rule_replaces_page_rule_and_unknown_pages_have_none()
        {
            Assert.Equal(new[] { Perm.InvoiceView }, Perm.Required("/Invoices/Index", null));
            Assert.Equal(new[] { Perm.InvoiceCollect }, Perm.Required("/Invoices/Index", "collect"));
            Assert.Equal(new[] { Perm.InvoiceView }, Perm.Required("/Invoices/Index", "Export"));
            Assert.Null(Perm.Required("/Brand/NewPage", null));
        }

        [Fact]
        public async Task Anonymous_users_are_sent_to_login_for_every_page()
        {
            var app = new TestApp();
            var c = app.NewClient();
            foreach (var page in PagePaths(app).Where(p => p != "/Login"))
            {
                var url = page.EndsWith("/Index") ? page[..^"/Index".Length] : page;
                if (url == "") url = "/";
                var res = await c.GetAsync(url);
                Assert.True(res.StatusCode == HttpStatusCode.Redirect && res.Headers.Location!.OriginalString.Contains("/Login"), $"{url} -> {(int)res.StatusCode}");
            }
        }

        // ---- enforcement ------------------------------------------------------------------------------------

        [Fact]
        public async Task User_without_a_role_sees_only_notices()
        {
            var (app, _) = NewApp();
            app.SeedUser("nobody", "pw", "Nina", role: null);
            var c = await LoginAs(app, "nobody");

            var home = await c.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, home.StatusCode);
            var html = await home.Content.ReadAsStringAsync();
            Assert.Contains("href=\"/\"", html);
            foreach (var link in new[] { "/Users", "/Roles", "/Invoices", "/Requisitions", "/Reports", "/Dashboard", "/Products" })
                Assert.DoesNotContain($"href=\"{link}\"", html);

            foreach (var url in new[] { "/Users", "/Roles", "/Invoices", "/Requisitions", "/Reports", "/Dashboard", "/Products" })
                Assert.True(Denied(await c.GetAsync(url)), url);

            var denied = await c.GetAsync("/AccessDenied");
            Assert.Equal(HttpStatusCode.Forbidden, denied.StatusCode);
            Assert.Contains("do not have access", await denied.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Htmx_requests_get_403_with_a_toast_not_a_redirect()
        {
            var (app, _) = NewApp();
            app.SeedUser("nobody", "pw", "Nina", role: null);
            var c = await LoginAs(app, "nobody");
            var req = new HttpRequestMessage(HttpMethod.Get, "/Users?handler=List");
            req.Headers.Add("HX-Request", "true");
            var res = await c.SendAsync(req);
            Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
            Assert.Contains("permission", res.Trigger());
        }

        private static int SeedInvoice(TestApp app, int userId, double qty = 10)
        {
            var good = app.SeedGood("Soap", 10);
            app.SeedOpenWindow();
            var reqId = app.Run(sp => sp.GetRequiredService<RequisitionService>().Create(userId, new[] { new RequisitionLine(good, qty) })).Requisition.Id;
            return app.Run(sp => sp.GetRequiredService<InvoiceService>().CreateFromRequisitions(new[] { reqId })).Invoices[0].Id;
        }

        [Fact]
        public async Task Menu_and_buttons_follow_permissions()
        {
            var (app, adminId) = NewApp();
            SeedInvoice(app, adminId);
            app.SeedRole("Viewer", Perm.InvoiceView);
            app.SeedUser("viewer", "pw", "Vic", role: "Viewer");
            var c = await LoginAs(app, "viewer");

            var html = await c.GetStringAsync("/Invoices");
            Assert.Contains("href=\"/Invoices\"", html);
            foreach (var link in new[] { "/Users", "/Collections", "/Requisitions", "/Requisitions/Create", "/Roles" })
                Assert.DoesNotContain($"href=\"{link}\"", html);
            Assert.Contains("INV - 000001", html);
            Assert.DoesNotContain("handler=Panel", html);          // no Collect button without the permission
        }

        [Fact]
        public async Task Action_permission_is_separate_from_view_permission()
        {
            var (app, adminId) = NewApp();
            var good = app.SeedGood("Soap", 10);
            app.SeedOpenWindow();
            var admin = await LoginAs(app, "admin");
            await admin.FormPost("/Requisitions/Create", "/Requisitions/Create", new() { ["UserId"] = adminId.ToString(), [$"Qty[{good}]"] = "5" });

            app.SeedRole("Clerk", Perm.RequisitionView, Perm.InvoiceView);
            app.SeedUser("clerk", "pw", "Cleo", role: "Clerk");
            var c = await LoginAs(app, "clerk");

            var board = await c.GetStringAsync("/Requisitions");
            Assert.Contains("REQ - 000001", board);
            Assert.DoesNotContain("handler=Invoice&amp;id=1", board);   // no Invoice button
            Assert.DoesNotContain("handler=Invoice&id=1", board);

            var approve = await c.FormPost("/Requisitions", "/Requisitions?handler=Invoice&id=1", new());
            Assert.True(Denied(approve));
            Assert.Contains("REQ - 000001", await admin.GetStringAsync("/Requisitions"));   // still waiting: nothing was invoiced

            // after the role gains the permission, the same session may invoice (no re-login needed)
            app.SeedRole("Clerk", Perm.RequisitionView, Perm.InvoiceView, Perm.RequisitionApprove);
            var ok = await c.FormPost("/Requisitions", "/Requisitions?handler=Invoice&id=1", new());
            Assert.Equal(HttpStatusCode.Redirect, ok.StatusCode);
            Assert.Contains("created", await c.FlashAfter(ok));
        }

        [Fact]
        public async Task Collect_is_denied_without_the_collect_permission_even_when_the_handler_is_spoofed()
        {
            var (app, adminId) = NewApp();
            var invId = SeedInvoice(app, adminId);

            app.SeedRole("Viewer", Perm.InvoiceView);
            app.SeedUser("viewer", "pw", "Vic", role: "Viewer");
            var c = await LoginAs(app, "viewer");

            Assert.True(Denied(await c.GetAsync($"/Invoices?handler=Panel&id={invId}")));
            var direct = await c.FormPost("/Invoices", "/Invoices?handler=Collect", new() { ["invoiceId"] = invId.ToString(), ["amount"] = "50", ["method"] = "Cash" });
            Assert.True(Denied(direct));

            // spoof: no handler in the URL, handler named in the form body
            await c.FormPost("/Invoices", "/Invoices", new() { ["handler"] = "Collect", ["invoiceId"] = invId.ToString(), ["amount"] = "50", ["method"] = "Cash" });

            var detail = app.Run(sp => sp.GetRequiredService<InvoiceService>().Get(invId));
            Assert.Equal(0, detail.Header.Collected);   // nothing was collected by any route
        }

        [Fact]
        public async Task Dashboard_permission_does_not_open_the_reports_page()
        {
            var (app, _) = NewApp();
            app.SeedRole("Boss", Perm.Dashboard);
            app.SeedUser("boss", "pw", "Bo", role: "Boss");
            var c = await LoginAs(app, "boss");
            Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/Dashboard")).StatusCode);
            Assert.True(Denied(await c.GetAsync("/Reports")));
            Assert.True(Denied(await c.GetAsync("/Reports?handler=Export")));
        }

        [Fact]
        public async Task Sign_in_lands_on_the_dashboard_only_for_users_who_may_see_it()
        {
            var (app, _) = NewApp();
            app.SeedRole("Boss", Perm.Dashboard);
            app.SeedUser("boss", "pw", "Bo", role: "Boss");
            app.SeedUser("nobody", "pw", "Nina", role: null);
            Assert.Equal("/Dashboard", (await TestApp.Login(app.NewClient(), "boss", "pw")).Headers.Location!.OriginalString);
            Assert.Equal("/", (await TestApp.Login(app.NewClient(), "nobody", "pw")).Headers.Location!.OriginalString);
        }

        [Fact]
        public async Task A_deleted_user_loses_access_immediately()
        {
            var (app, _) = NewApp();
            app.SeedRole("Viewer", Perm.InvoiceView);
            var id = app.SeedUser("viewer", "pw", "Vic", role: "Viewer");
            var c = await LoginAs(app, "viewer");
            Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/Invoices")).StatusCode);

            app.Seed(db => db.Users.Single(u => u.Id == id).IsDeleted = true);
            var res = await c.GetAsync("/Invoices");
            Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
            Assert.Contains("/Login", res.Headers.Location!.OriginalString);
        }

        // ---- roles page -------------------------------------------------------------------------------------

        private static async Task<HttpResponseMessage> PostPairs(HttpClient c, string pageUrl, string url, params (string Key, string Value)[] pairs)
        {
            var all = pairs.Select(p => KeyValuePair.Create(p.Key, p.Value)).ToList();
            all.Add(KeyValuePair.Create("__RequestVerificationToken", await TestApp.Antiforgery(c, pageUrl)));
            return await c.PostAsync(url, new FormUrlEncodedContent(all));
        }

        [Fact]
        public async Task Roles_page_assigns_permissions_replacing_the_set()
        {
            var (app, _) = NewApp();
            var c = await LoginAs(app, "admin");
            await c.FormPost("/Roles", "/Roles?handler=CreateRole", new() { ["name"] = "Clerk" });
            var roleId = RoleId(app, "Clerk");

            Task<HttpResponseMessage> Assign(params string[] keys) =>
                PostPairs(c, $"/Roles?role={roleId}", "/Roles?handler=Assign", keys.Select(k => ("Assign.Keys", k)).Prepend(("Assign.RoleId", roleId.ToString())).ToArray());

            Assert.Equal("Permissions saved.", await c.FlashAfter(await Assign(Perm.InvoiceView, Perm.Reports)));
            var page = await c.GetStringAsync($"/Roles?role={roleId}");
            Assert.Contains("value=\"Reports.View\" checked", page);
            Assert.Contains("value=\"Invoices.View\" checked", page);
            Assert.Contains("Sales invoice", page);       // permissions are grouped

            await Assign(Perm.Reports, Perm.Reports);   // duplicate posted keys must not duplicate rows; Invoices.View removed
            page = await c.GetStringAsync($"/Roles?role={roleId}");
            Assert.DoesNotContain("value=\"Invoices.View\" checked", page);
            Assert.Equal(1, app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().RoleWisePermissions.Count(p => p.RoleId == roleId && !p.IsDeleted)));

            // the Administrator role is built in
            var adminRoleId = RoleId(app, "Administrator");
            var adminTry = await PostPairs(c, "/Roles", "/Roles?handler=Assign", ("Assign.RoleId", adminRoleId.ToString()), ("Assign.Keys", Perm.Reports));
            Assert.Contains("full access", await c.FlashAfter(adminTry));
            Assert.Contains("all access", await c.GetStringAsync("/Roles"));
        }

        [Fact]
        public async Task A_non_administrator_cannot_grant_permissions_they_do_not_hold()
        {
            var (app, _) = NewApp();
            app.SeedRole("RoleManager", Perm.Roles, Perm.Reports);
            app.SeedRole("Target");
            app.SeedUser("mgr", "pw", "Max", role: "RoleManager");
            var c = await LoginAs(app, "mgr");
            var roleId = RoleId(app, "Target");

            await PostPairs(c, "/Roles", "/Roles?handler=Assign", ("Assign.RoleId", roleId.ToString()),
                ("Assign.Keys", Perm.Reports), ("Assign.Keys", Perm.Users), ("Assign.Keys", Perm.InvoiceCollect));

            var granted = app.Run(sp =>
            {
                var db = sp.GetRequiredService<ApplicationDbContext>();
                return db.RoleWisePermissions.Where(p => p.RoleId == roleId).Join(db.ApplicationModules, p => p.ModuleId, m => m.Id, (p, m) => m.Url).ToList();
            });
            Assert.Equal(new[] { Perm.Reports }, granted);   // Users and Invoices.Collect were silently not granted

            Assert.Contains("you do not hold this permission", await c.GetStringAsync($"/Roles?role={roleId}"));
        }

        // ---- user roles -------------------------------------------------------------------------------------

        private static int RoleId(TestApp app, string name) =>
            app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().ApplicationRoles.Single(r => r.Name == name).Id);

        [Fact]
        public async Task Admin_assigns_a_role_through_the_edit_panel_and_it_takes_effect_at_once()
        {
            var (app, _) = NewApp();
            app.SeedRole("Viewer", Perm.InvoiceView);
            var viewer = app.SeedUser("viewer", "pw", "Vic", role: null);
            var v = await LoginAs(app, "viewer");
            Assert.True(Denied(await v.GetAsync("/Invoices")));

            var admin = await LoginAs(app, "admin");
            Assert.Contains("name=\"Edit.RoleId\"", await admin.GetStringAsync($"/Users?editId={viewer}"));
            var res = await admin.FormPost($"/Users?editId={viewer}", "/Users?handler=Edit", new() { ["Edit.Id"] = viewer.ToString(), ["Edit.FirstName"] = "Vic", ["Edit.RoleId"] = RoleId(app, "Viewer").ToString() });
            Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);

            Assert.Equal(HttpStatusCode.OK, (await v.GetAsync("/Invoices")).StatusCode);
            Assert.Contains("Viewer", await admin.GetStringAsync("/Users"));

            // removing the role removes the access again
            await admin.FormPost($"/Users?editId={viewer}", "/Users?handler=Edit", new() { ["Edit.Id"] = viewer.ToString(), ["Edit.FirstName"] = "Vic", ["Edit.RoleId"] = "0" });
            Assert.True(Denied(await v.GetAsync("/Invoices")));
        }

        [Fact]
        public async Task Cannot_escalate_to_administrator_or_assign_roles_without_the_roles_permission()
        {
            var (app, adminId) = NewApp();
            app.SeedRole("UserAdmin", Perm.Users, Perm.Roles);
            app.SeedRole("UsersOnly", Perm.Users);
            app.SeedRole("Viewer", Perm.InvoiceView);
            var ua = app.SeedUser("ua", "pw", "Uma", role: "UserAdmin");
            var uo = app.SeedUser("uo", "pw", "Ola", role: "UsersOnly");
            var adminRole = RoleId(app, "Administrator");

            // a user manager with the Roles permission cannot see or grant Administrator
            var c = await LoginAs(app, "ua");
            var form = await c.GetStringAsync($"/Users?editId={ua}");
            Assert.Contains(">Viewer</option>", form);
            Assert.DoesNotContain(">Administrator</option>", form);
            var self = await c.FormPost($"/Users?editId={ua}", "/Users?handler=Edit", new() { ["Edit.Id"] = ua.ToString(), ["Edit.FirstName"] = "Uma", ["Edit.RoleId"] = adminRole.ToString() });
            Assert.Equal(HttpStatusCode.OK, self.StatusCode);
            Assert.Contains("Only administrators", await self.Content.ReadAsStringAsync());
            Assert.False(app.Run(sp => sp.GetRequiredService<AccessService>().For(ua).IsAdmin));

            // nor can they demote an administrator
            var demote = await c.FormPost($"/Users?editId={adminId}", "/Users?handler=Edit", new() { ["Edit.Id"] = adminId.ToString(), ["Edit.FirstName"] = "Ada", ["Edit.RoleId"] = RoleId(app, "Viewer").ToString() });
            Assert.Contains("Only administrators", await demote.Content.ReadAsStringAsync());

            // creating a user with the Administrator role is refused and creates nothing
            app.Seed(db => db.UserTypes.Add(new TalukdarSales.Web.Models.UserType { TypeName = "T", CreatedOn = DateTime.Now }));
            var before = app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().Users.Count());
            var create = await c.FormPost("/Users?new=true", "/Users?handler=Create", new()
                { ["Create.UserTypeId"] = "1", ["Create.FirstName"] = "Evil", ["Create.PhoneNumber"] = "1", ["Create.RoleId"] = adminRole.ToString() });
            Assert.Equal(HttpStatusCode.OK, create.StatusCode);
            Assert.Equal(before, app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().Users.Count()));

            // a user manager WITHOUT the Roles permission has no role field and a forged one is rejected
            var c2 = await LoginAs(app, "uo");
            Assert.DoesNotContain("name=\"Edit.RoleId\"", await c2.GetStringAsync($"/Users?editId={uo}"));
            var forged = await c2.FormPost($"/Users?editId={uo}", "/Users?handler=Edit", new() { ["Edit.Id"] = uo.ToString(), ["Edit.FirstName"] = "Ola", ["Edit.RoleId"] = RoleId(app, "Viewer").ToString() });
            Assert.Contains("do not have permission", await forged.Content.ReadAsStringAsync());
            Assert.False(app.Run(sp => sp.GetRequiredService<AccessService>().For(uo).Has(Perm.InvoiceView)));
        }

        [Fact]
        public async Task The_last_administrator_cannot_be_demoted()
        {
            var (app, adminId) = NewApp();
            var c = await LoginAs(app, "admin");
            app.SeedRole("Viewer", Perm.InvoiceView);

            var blocked = await c.FormPost($"/Users?editId={adminId}", "/Users?handler=Edit", new() { ["Edit.Id"] = adminId.ToString(), ["Edit.FirstName"] = "Ada", ["Edit.RoleId"] = RoleId(app, "Viewer").ToString() });
            Assert.Contains("last administrator", await blocked.Content.ReadAsStringAsync());
            Assert.True(app.Run(sp => sp.GetRequiredService<AccessService>().For(adminId).IsAdmin));

            // once a second administrator exists, the first can step down
            app.SeedUser("admin2", "pw", "Bea");
            var ok = await c.FormPost($"/Users?editId={adminId}", "/Users?handler=Edit", new() { ["Edit.Id"] = adminId.ToString(), ["Edit.FirstName"] = "Ada", ["Edit.RoleId"] = RoleId(app, "Viewer").ToString() });
            Assert.Equal(HttpStatusCode.Redirect, ok.StatusCode);
            Assert.False(app.Run(sp => sp.GetRequiredService<AccessService>().For(adminId).IsAdmin));
        }

        // ---- seeding ----------------------------------------------------------------------------------------

        private static void RunSeeder(TestApp app, Dictionary<string, string> config = null)
        {
            using var scope = app.Services.CreateScope();
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(config ?? new()).Build();
            new AccessSeeder(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(), cfg, NullLogger<AccessSeeder>.Instance).Run();
        }

        [Fact]
        public void Seeder_creates_the_catalog_once_and_is_idempotent()
        {
            var app = new TestApp();
            RunSeeder(app); RunSeeder(app);
            var db = app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().ApplicationModules.Select(m => m.Url).ToList());
            Assert.Equal(Perm.Catalog.Count, db.Count(u => Perm.Find(u) != null));
            Assert.Equal(db.Count, db.Distinct().Count());
            Assert.Single(app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().ApplicationRoles.Where(r => r.Name == "Administrator").ToList()));
        }

        [Fact]
        public void First_start_promotes_the_oldest_user_or_the_configured_one_and_only_once()
        {
            var app = new TestApp();
            var first = app.SeedUser("owner", "pw", role: null);
            var second = app.SeedUser("other", "pw", role: null);

            RunSeeder(app, new() { ["Security:InitialAdmin"] = "other" });
            Assert.True(app.Run(sp => sp.GetRequiredService<AccessService>().For(second).IsAdmin));
            Assert.False(app.Run(sp => sp.GetRequiredService<AccessService>().For(first).IsAdmin));

            RunSeeder(app);   // an administrator exists now: nobody else is promoted
            Assert.False(app.Run(sp => sp.GetRequiredService<AccessService>().For(first).IsAdmin));

            var app2 = new TestApp();
            var oldest = app2.SeedUser("owner", "pw", role: null);
            app2.SeedUser("later", "pw", role: null);
            RunSeeder(app2);
            Assert.True(app2.Run(sp => sp.GetRequiredService<AccessService>().For(oldest).IsAdmin));
        }
    }
}
