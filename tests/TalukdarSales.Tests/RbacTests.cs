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
            foreach (var key in Perm.Rules.SelectMany(r => r.AnyOf))
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
            Assert.DoesNotContain("User Management", html);
            Assert.DoesNotContain("Invoice List", html);

            foreach (var url in new[] { "/Users", "/Roles", "/Invoices", "/Requisitions", "/Reports", "/Dashboard", "/Products", "/Lookup?handler=Users" })
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

        [Fact]
        public async Task Menu_and_buttons_follow_permissions()
        {
            var (app, _) = NewApp();
            app.SeedRole("Viewer", Perm.InvoiceView);
            app.SeedUser("viewer", "pw", "Vic", role: "Viewer");
            var c = await LoginAs(app, "viewer");

            var html = await c.GetStringAsync("/Invoices");
            Assert.Contains("Invoice List", html);
            Assert.DoesNotContain("User Management", html);
            Assert.DoesNotContain("Collection History", html);
            Assert.DoesNotContain("Requisition List", html);
            Assert.DoesNotContain("Invoice Form", html);
        }

        [Fact]
        public async Task Action_permission_is_separate_from_view_permission()
        {
            var (app, adminId) = NewApp();
            var good = app.SeedGood("Soap", 10);
            app.SeedOpenWindow();
            var admin = await LoginAs(app, "admin");
            await admin.PostAsync("/Requisitions/Create", new FormUrlEncodedContent(new Dictionary<string, string>
                { ["UserId"] = adminId.ToString(), [$"Qty[{good}]"] = "5", ["__RequestVerificationToken"] = await TestApp.Antiforgery(admin, "/Requisitions/Create") }));

            app.SeedRole("Clerk", Perm.RequisitionView, Perm.InvoiceView);
            app.SeedUser("clerk", "pw", "Cleo", role: "Clerk");
            var c = await LoginAs(app, "clerk");

            Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/Requisitions")).StatusCode);
            var list = await c.GetStringAsync("/Requisitions?handler=List");
            Assert.DoesNotContain("Approve Selected", list);

            var approve = await c.HtmxPost("/Requisitions", "/Requisitions?handler=Approve", new() { ["ids"] = "1", ["Active"] = "true" });
            Assert.Equal(HttpStatusCode.Forbidden, approve.StatusCode);
            Assert.Contains("REQ - 000001", await admin.GetStringAsync("/Requisitions?handler=List"));   // still active: nothing was invoiced

            // after the role gains the permission, the same session may approve (no re-login needed)
            app.SeedRole("Clerk", Perm.RequisitionView, Perm.InvoiceView, Perm.RequisitionApprove);
            var ok = await c.HtmxPost("/Requisitions", "/Requisitions?handler=Approve", new() { ["ids"] = "1", ["Active"] = "true" });
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
            Assert.Contains("invoice(s) created", ok.Trigger());
        }

        [Fact]
        public async Task Collect_is_denied_without_the_collect_permission_even_when_the_handler_is_spoofed()
        {
            var (app, adminId) = NewApp();
            var good = app.SeedGood("Soap", 10);
            app.SeedOpenWindow();
            var reqId = app.Run(sp => sp.GetRequiredService<RequisitionService>().Create(adminId, new[] { new RequisitionLine(good, 10) })).Requisition.Id;
            var invId = app.Run(sp => sp.GetRequiredService<InvoiceService>().CreateFromRequisitions(new[] { reqId })).Invoices[0].Id;

            app.SeedRole("Viewer", Perm.InvoiceView);
            app.SeedUser("viewer", "pw", "Vic", role: "Viewer");
            var c = await LoginAs(app, "viewer");

            Assert.True(Denied(await c.GetAsync($"/Invoices?handler=Collect&id={invId}")));
            var direct = await c.HtmxPost("/Invoices", "/Invoices?handler=Collect", new() { ["Collect.InvoiceId"] = invId.ToString(), ["Collect.Amount"] = "50", ["Collect.PaymentMethod"] = "Cash" });
            Assert.Equal(HttpStatusCode.Forbidden, direct.StatusCode);

            // spoof: no handler in the URL, handler named in the form body
            var spoof = await c.HtmxPost("/Invoices", "/Invoices", new() { ["handler"] = "Collect", ["Collect.InvoiceId"] = invId.ToString(), ["Collect.Amount"] = "50", ["Collect.PaymentMethod"] = "Cash" });
            Assert.DoesNotContain("Collected", spoof.Trigger());

            var detail = app.Run(sp => sp.GetRequiredService<InvoiceService>().Get(invId));
            Assert.Equal(0, detail.Header.Collected);   // nothing was collected by any route
        }

        [Fact]
        public async Task Dashboard_permission_allows_the_report_widgets_but_not_the_reports_page()
        {
            var (app, _) = NewApp();
            app.SeedRole("Boss", Perm.Dashboard);
            app.SeedUser("boss", "pw", "Bo", role: "Boss");
            var c = await LoginAs(app, "boss");
            Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/Dashboard")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/Reports?handler=Panel&kind=sellers")).StatusCode);
            Assert.True(Denied(await c.GetAsync("/Reports")));
            Assert.True(Denied(await c.GetAsync("/Reports?handler=Export&kind=sellers")));
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

        [Fact]
        public async Task Roles_page_assigns_permissions_replacing_the_set()
        {
            var (app, _) = NewApp();
            var c = await LoginAs(app, "admin");
            await c.HtmxPost("/Roles", "/Roles?handler=CreateRole", new() { ["Role.Name"] = "Clerk" });
            var roleId = app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().ApplicationRoles.Single(r => r.Name == "Clerk").Id);

            async Task<HttpResponseMessage> Assign(params string[] keys)
            {
                var form = new MultipartFormDataContent { { new StringContent(await TestApp.Antiforgery(c, "/Roles")), "__RequestVerificationToken" }, { new StringContent(roleId.ToString()), "Assign.RoleId" } };
                foreach (var k in keys) form.Add(new StringContent(k), "Assign.Keys");
                var req = new HttpRequestMessage(HttpMethod.Post, "/Roles?handler=Assign") { Content = form };
                req.Headers.Add("HX-Request", "true");
                return await c.SendAsync(req);
            }

            Assert.Contains("closeModal", (await Assign(Perm.InvoiceView, Perm.Reports)).Trigger());
            Assert.Contains("View reports", await c.GetStringAsync("/Roles?handler=List"));
            Assert.Contains("View, print and export invoices", await c.GetStringAsync("/Roles?handler=List"));

            await Assign(Perm.Reports, Perm.Reports);   // duplicate posted keys must not duplicate rows; Invoices.View removed
            var list = await c.GetStringAsync("/Roles?handler=List");
            Assert.DoesNotContain("View, print and export invoices", list);
            Assert.Equal(1, app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().RoleWisePermissions.Count(p => p.RoleId == roleId && !p.IsDeleted)));

            // the form shows grouped permissions with the current ones ticked
            var form2 = await c.GetStringAsync($"/Roles?handler=Assign&id={roleId}");
            Assert.Contains("Sales invoice", form2);
            Assert.Contains("value=\"Reports.View\" checked", form2);

            // the Administrator role is built in
            var adminRoleId = app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().ApplicationRoles.Single(r => r.Name == "Administrator").Id);
            var adminTry = await c.HtmxPost("/Roles", "/Roles?handler=Assign", new() { ["Assign.RoleId"] = adminRoleId.ToString(), ["Assign.Keys"] = Perm.Reports });
            Assert.Contains("full access", adminTry.Trigger());
            Assert.Contains("All access", await c.GetStringAsync("/Roles?handler=List"));
        }

        [Fact]
        public async Task A_non_administrator_cannot_grant_permissions_they_do_not_hold()
        {
            var (app, _) = NewApp();
            app.SeedRole("RoleManager", Perm.Roles, Perm.Reports);
            app.SeedRole("Target");
            app.SeedUser("mgr", "pw", "Max", role: "RoleManager");
            var c = await LoginAs(app, "mgr");
            var roleId = app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().ApplicationRoles.Single(r => r.Name == "Target").Id);

            var form = new MultipartFormDataContent { { new StringContent(await TestApp.Antiforgery(c, "/Roles")), "__RequestVerificationToken" }, { new StringContent(roleId.ToString()), "Assign.RoleId" },
                { new StringContent(Perm.Reports), "Assign.Keys" }, { new StringContent(Perm.Users), "Assign.Keys" }, { new StringContent(Perm.InvoiceCollect), "Assign.Keys" } };
            var req = new HttpRequestMessage(HttpMethod.Post, "/Roles?handler=Assign") { Content = form };
            req.Headers.Add("HX-Request", "true");
            await c.SendAsync(req);

            var granted = app.Run(sp =>
            {
                var db = sp.GetRequiredService<ApplicationDbContext>();
                return db.RoleWisePermissions.Where(p => p.RoleId == roleId).Join(db.ApplicationModules, p => p.ModuleId, m => m.Id, (p, m) => m.Url).ToList();
            });
            Assert.Equal(new[] { Perm.Reports }, granted);   // Users and Invoices.Collect were silently not granted

            Assert.Contains("(you do not hold this permission)", await c.GetStringAsync($"/Roles?handler=Assign&id={roleId}"));
        }

        // ---- user roles -------------------------------------------------------------------------------------

        private static int RoleId(TestApp app, string name) =>
            app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().ApplicationRoles.Single(r => r.Name == name).Id);

        [Fact]
        public async Task Admin_assigns_a_role_through_the_edit_modal_and_it_takes_effect_at_once()
        {
            var (app, _) = NewApp();
            app.SeedRole("Viewer", Perm.InvoiceView);
            var viewer = app.SeedUser("viewer", "pw", "Vic", role: null);
            var v = await LoginAs(app, "viewer");
            Assert.True(Denied(await v.GetAsync("/Invoices")));

            var admin = await LoginAs(app, "admin");
            Assert.Contains("name=\"Edit.RoleId\"", await admin.GetStringAsync($"/Users?handler=Edit&id={viewer}"));
            var res = await admin.HtmxPost("/Users", "/Users?handler=Edit", new() { ["Edit.Id"] = viewer.ToString(), ["Edit.FirstName"] = "Vic", ["Edit.RoleId"] = RoleId(app, "Viewer").ToString() });
            Assert.Contains("closeModal", res.Trigger());

            Assert.Equal(HttpStatusCode.OK, (await v.GetAsync("/Invoices")).StatusCode);
            Assert.Contains("Viewer", await admin.GetStringAsync("/Users?handler=List"));

            // removing the role removes the access again
            await admin.HtmxPost("/Users", "/Users?handler=Edit", new() { ["Edit.Id"] = viewer.ToString(), ["Edit.FirstName"] = "Vic", ["Edit.RoleId"] = "0" });
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
            var form = await c.GetStringAsync($"/Users?handler=Edit&id={ua}");
            Assert.Contains(">Viewer<", form);
            Assert.DoesNotContain(">Administrator<", form);
            var self = await c.HtmxPost("/Users", "/Users?handler=Edit", new() { ["Edit.Id"] = ua.ToString(), ["Edit.FirstName"] = "Uma", ["Edit.RoleId"] = adminRole.ToString() });
            Assert.Equal("", self.Trigger());
            Assert.Contains("Only administrators", await self.Content.ReadAsStringAsync());
            Assert.False(app.Run(sp => sp.GetRequiredService<AccessService>().For(ua).IsAdmin));

            // nor can they demote an administrator
            var demote = await c.HtmxPost("/Users", "/Users?handler=Edit", new() { ["Edit.Id"] = adminId.ToString(), ["Edit.FirstName"] = "Ada", ["Edit.RoleId"] = RoleId(app, "Viewer").ToString() });
            Assert.Contains("Only administrators", await demote.Content.ReadAsStringAsync());

            // creating a user with the Administrator role is refused and creates nothing
            app.Seed(db => db.UserTypes.Add(new TalukdarSales.Web.Models.UserType { TypeName = "T", CreatedOn = DateTime.Now }));
            var before = app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().Users.Count());
            var create = await c.HtmxPost("/Users", "/Users?handler=Create", new()
                { ["Create.UserTypeId"] = "1", ["Create.FirstName"] = "Evil", ["Create.PhoneNumber"] = "1", ["Create.RoleId"] = adminRole.ToString() });
            Assert.Equal("", create.Trigger());
            Assert.Equal(before, app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().Users.Count()));

            // a user manager WITHOUT the Roles permission has no role field and a forged one is rejected
            var c2 = await LoginAs(app, "uo");
            Assert.DoesNotContain("name=\"Edit.RoleId\"", await c2.GetStringAsync($"/Users?handler=Edit&id={uo}"));
            var forged = await c2.HtmxPost("/Users", "/Users?handler=Edit", new() { ["Edit.Id"] = uo.ToString(), ["Edit.FirstName"] = "Ola", ["Edit.RoleId"] = RoleId(app, "Viewer").ToString() });
            Assert.Contains("do not have permission", await forged.Content.ReadAsStringAsync());
            Assert.False(app.Run(sp => sp.GetRequiredService<AccessService>().For(uo).Has(Perm.InvoiceView)));
        }

        [Fact]
        public async Task The_last_administrator_cannot_be_demoted()
        {
            var (app, adminId) = NewApp();
            var c = await LoginAs(app, "admin");
            app.SeedRole("Viewer", Perm.InvoiceView);

            var blocked = await c.HtmxPost("/Users", "/Users?handler=Edit", new() { ["Edit.Id"] = adminId.ToString(), ["Edit.FirstName"] = "Ada", ["Edit.RoleId"] = RoleId(app, "Viewer").ToString() });
            Assert.Contains("last administrator", await blocked.Content.ReadAsStringAsync());
            Assert.True(app.Run(sp => sp.GetRequiredService<AccessService>().For(adminId).IsAdmin));

            // once a second administrator exists, the first can step down
            app.SeedUser("admin2", "pw", "Bea");
            var ok = await c.HtmxPost("/Users", "/Users?handler=Edit", new() { ["Edit.Id"] = adminId.ToString(), ["Edit.FirstName"] = "Ada", ["Edit.RoleId"] = RoleId(app, "Viewer").ToString() });
            Assert.Contains("closeModal", ok.Trigger());
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
