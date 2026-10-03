using Xunit;

namespace TalukdarSales.Tests
{
    public class ProductPagesTests : IClassFixture<TestApp>
    {
        private readonly TestApp _app;
        public ProductPagesTests(TestApp app) { _app = app; _app.SeedUser(); }

        private async Task<HttpClient> LoggedIn()
        {
            var c = _app.NewClient();
            await TestApp.Login(c, "admin", "secret123");
            return c;
        }

        [Fact]
        public async Task Category_create_rejects_duplicates()
        {
            var c = await LoggedIn();
            var first = await c.FormPost("/Products", "/Products?handler=CreateCategory", new() { ["name"] = "Biscuit" });
            Assert.Equal("Category added.", await c.FlashAfter(first));
            var dup = await c.FormPost("/Products", "/Products?handler=CreateCategory", new() { ["name"] = "biscuit" });
            Assert.Contains("already exists", await c.FlashAfter(dup));
            Assert.Contains("Biscuit", await c.GetStringAsync("/Products"));
        }

        [Fact]
        public async Task Product_create_edit_toggle_and_filter()
        {
            var c = await LoggedIn();
            await c.FormPost("/Products", "/Products?handler=CreateCategory", new() { ["name"] = "Noodles" });

            var created = await c.HtmxPost("/Products?new=true", "/Products?handler=Create", new()
            {
                ["Create.Name"] = "Chicken Noodles", ["Create.UOM"] = "pack", ["Create.GoodTypeId"] = "1",
                ["Create.Description"] = "Spicy", ["Create.UnitPrice"] = "25.5"
            });
            Assert.Equal("Product added.", await c.FlashAfter(created));
            Assert.Contains("Chicken Noodles", await c.GetStringAsync("/Products?name=noodle"));
            Assert.DoesNotContain("Chicken Noodles", await c.GetStringAsync("/Products?name=zzz"));

            var edit = await c.HtmxPost("/Products?editId=1", "/Products?handler=Edit", new()
            {
                ["Edit.Id"] = "1", ["Edit.Description"] = "Mild", ["Edit.UnitPrice"] = "30"
            });
            Assert.Equal("Product updated.", await c.FlashAfter(edit));
            var list = await c.GetStringAsync("/Products");
            Assert.Contains("30", list);
            Assert.Contains("Hidden", list);   // IsActive checkbox was not posted

            var toggle = await c.FormPost("/Products", "/Products?handler=Toggle&id=1", new());
            Assert.Contains("available again", await c.FlashAfter(toggle));
        }

        [Fact]
        public async Task Product_requires_type_and_name()
        {
            var c = await LoggedIn();
            var res = await c.HtmxPost("/Products?new=true", "/Products?handler=Create", new() { ["Create.UOM"] = "kg" });
            var body = await res.Content.ReadAsStringAsync();
            Assert.Contains("Add product", body);
            Assert.Contains("field-validation-error", body);
        }
    }
}
