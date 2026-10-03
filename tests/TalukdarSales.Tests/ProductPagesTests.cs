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
        public async Task Product_type_create_rejects_duplicates()
        {
            var c = await LoggedIn();
            var first = await c.HtmxPost("/ProductTypes", "/ProductTypes?handler=Create", new() { ["Create.Name"] = "Biscuit" });
            Assert.Contains("closeModal", first.Trigger());

            var dup = await c.HtmxPost("/ProductTypes", "/ProductTypes?handler=Create", new() { ["Create.Name"] = "Biscuit" });
            Assert.Equal("", dup.Trigger());
            Assert.Contains("already exists", await dup.Content.ReadAsStringAsync());

            Assert.Contains("Biscuit", await c.GetStringAsync("/ProductTypes?handler=List"));
        }

        [Fact]
        public async Task Product_create_edit_and_filter()
        {
            var c = await LoggedIn();
            await c.HtmxPost("/ProductTypes", "/ProductTypes?handler=Create", new() { ["Create.Name"] = "Noodles" });

            var created = await c.HtmxPost("/Products", "/Products?handler=Create", new()
            {
                ["Create.Name"] = "Chicken Noodles", ["Create.UOM"] = "pack", ["Create.GoodTypeId"] = "1",
                ["Create.Description"] = "Spicy", ["Create.UnitPrice"] = "25.5"
            });
            Assert.Contains("closeModal", created.Trigger());
            Assert.Contains("Chicken Noodles", await c.GetStringAsync("/Products?handler=List&Name=noodle"));
            Assert.DoesNotContain("Chicken Noodles", await c.GetStringAsync("/Products?handler=List&Name=zzz"));

            var edit = await c.HtmxPost("/Products", "/Products?handler=Edit", new()
            {
                ["Edit.Id"] = "1", ["Edit.Description"] = "Mild", ["Edit.UnitPrice"] = "30"
            });
            Assert.Contains("Product Updated", edit.Trigger());
            var list = await c.GetStringAsync("/Products?handler=List");
            Assert.Contains("30.00", list);
            Assert.Contains("Inactive", list);   // IsActive checkbox was not posted
        }

        [Fact]
        public async Task Product_requires_type_and_name()
        {
            var c = await LoggedIn();
            var res = await c.HtmxPost("/Products", "/Products?handler=Create", new() { ["Create.UOM"] = "kg" });
            Assert.Equal("", res.Trigger());
            var body = await res.Content.ReadAsStringAsync();
            Assert.Contains("field-validation-error", body);
        }
    }
}
