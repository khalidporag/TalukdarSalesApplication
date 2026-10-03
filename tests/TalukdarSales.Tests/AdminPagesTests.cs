using Xunit;

namespace TalukdarSales.Tests
{
    public class AdminPagesTests : IClassFixture<TestApp>
    {
        private readonly TestApp _app;
        public AdminPagesTests(TestApp app) { _app = app; _app.SeedUser(); }

        private async Task<HttpClient> LoggedIn()
        {
            var c = _app.NewClient();
            await TestApp.Login(c, "admin", "secret123");
            return c;
        }

        [Fact]
        public async Task Duplicate_role_is_rejected()
        {
            var c = await LoggedIn();
            await c.HtmxPost("/Roles", "/Roles?handler=CreateRole", new() { ["Role.Name"] = "Clerk" });
            var dup = await c.HtmxPost("/Roles", "/Roles?handler=CreateRole", new() { ["Role.Name"] = "clerk" });
            Assert.Equal("", dup.Trigger());
            Assert.Contains("already exists", await dup.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Notice_create_and_duplicate_title()
        {
            var c = await LoggedIn();
            var ok = await c.HtmxPost("/Notices", "/Notices?handler=Create", new() { ["Create.Title"] = "Eid holiday", ["Create.Description"] = "Closed" });
            Assert.Contains("closeModal", ok.Trigger());
            Assert.Contains("Eid holiday", await c.GetStringAsync("/Notices?handler=List"));

            var dup = await c.HtmxPost("/Notices", "/Notices?handler=Create", new() { ["Create.Title"] = "Eid holiday", ["Create.Description"] = "x" });
            Assert.Equal("", dup.Trigger());
            Assert.Contains("already exists", await dup.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Time_setting_saves_and_validates()
        {
            var c = await LoggedIn();
            var token = await TestApp.Antiforgery(c, "/TimeSetting");
            var bad = await c.PostAsync("/TimeSetting", new FormUrlEncodedContent(new Dictionary<string, string>
                { ["Input.From"] = "25:00", ["Input.To"] = "17:00", ["__RequestVerificationToken"] = token }));
            Assert.Contains("Use HH:mm", await bad.Content.ReadAsStringAsync());

            token = await TestApp.Antiforgery(c, "/TimeSetting");
            var ok = await c.PostAsync("/TimeSetting", new FormUrlEncodedContent(new Dictionary<string, string>
                { ["Input.From"] = "09:30", ["Input.To"] = "18:00", ["__RequestVerificationToken"] = token }));
            Assert.Equal(System.Net.HttpStatusCode.Redirect, ok.StatusCode);
            var page = await c.GetStringAsync("/TimeSetting");
            Assert.Contains("value=\"09:30\"", page);
        }
    }
}
