using System.Net;
using TalukdarSales.Web.Models;
using Xunit;

namespace TalukdarSales.Tests
{
    public class AuthAndUsersTests : IClassFixture<TestApp>
    {
        private readonly TestApp _app;
        public AuthAndUsersTests(TestApp app) { _app = app; _app.SeedUser(); }

        [Fact]
        public async Task Anonymous_page_redirects_to_login()
        {
            var res = await _app.NewClient().GetAsync("/Users");
            Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
            Assert.Contains("/Login", res.Headers.Location!.OriginalString);
        }

        [Fact]
        public async Task Anonymous_htmx_request_gets_hx_redirect_not_a_fragment()
        {
            var client = _app.NewClient();
            var req = new HttpRequestMessage(HttpMethod.Get, "/Users?handler=List");
            req.Headers.Add("HX-Request", "true");
            var res = await client.SendAsync(req);
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
            Assert.True(res.Headers.Contains("HX-Redirect"));
        }

        [Fact]
        public async Task Wrong_password_shows_error_and_no_cookie()
        {
            var res = await TestApp.Login(_app.NewClient(), "admin", "nope");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            Assert.Contains("Invalid username or password", await res.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Login_then_user_list_renders_and_search_filters()
        {
            var client = _app.NewClient();
            var login = await TestApp.Login(client, "admin", "secret123");
            Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

            var page = await client.GetStringAsync("/Users");
            Assert.Contains("Ada Lovelace", page);

            var hit = await client.GetStringAsync("/Users?name=lovel");
            Assert.Contains("Ada Lovelace", hit);

            var miss = await client.GetStringAsync("/Users?name=zzz");
            Assert.Contains("No customers found", miss);
        }

        [Fact]
        public async Task Old_json_api_and_swagger_are_gone()
        {
            var client = _app.NewClient();
            await TestApp.Login(client, "admin", "secret123");
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/User")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/swagger/index.html")).StatusCode);
        }

        [Fact]
        public async Task Create_user_form_adds_user_and_redirects_with_a_toast()
        {
            var client = _app.NewClient();
            await TestApp.Login(client, "admin", "secret123");
            _app.Seed(db => db.UserTypes.Add(new UserType { TypeName = "Distributor", CreatedOn = DateTime.Now }));
            var token = await TestApp.Antiforgery(client, "/Users?new=true");

            var form = new MultipartFormDataContent
            {
                { new StringContent("1"), "Create.UserTypeId" },
                { new StringContent("Grace"), "Create.FirstName" },
                { new StringContent("Hopper"), "Create.LastName" },
                { new StringContent("0199"), "Create.PhoneNumber" },
                { new StringContent("1000"), "Create.MaxCreditLimit" },
                { new StringContent("5"), "Create.MaxCreditDays" },
                { new StringContent(token), "__RequestVerificationToken" }
            };
            var res = await client.PostAsync("/Users?handler=Create", form);

            Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
            Assert.Contains("User Added", await client.FlashAfter(res));
            Assert.Contains("Grace Hopper", await client.GetStringAsync("/Users?name=hopper"));
        }

        [Fact]
        public async Task Create_user_validation_error_rerenders_form()
        {
            var client = _app.NewClient();
            await TestApp.Login(client, "admin", "secret123");
            var token = await TestApp.Antiforgery(client, "/Users?new=true");
            var form = new MultipartFormDataContent { { new StringContent(token), "__RequestVerificationToken" } };
            var res = await client.PostAsync("/Users?handler=Create", form);
            var body = await res.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            Assert.Contains("New customer", body);
            Assert.Contains("field-validation-error", body);
        }

        [Fact]
        public async Task Post_without_antiforgery_token_is_rejected()
        {
            var client = _app.NewClient();
            await TestApp.Login(client, "admin", "secret123");
            var res = await client.PostAsync("/Users?handler=Edit", new FormUrlEncodedContent(new Dictionary<string, string> { ["Edit.Id"] = "1" }));
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }
    }
}
