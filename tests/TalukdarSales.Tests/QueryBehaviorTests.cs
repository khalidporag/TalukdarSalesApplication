using Microsoft.Extensions.DependencyInjection;
using TalukdarSales.Web.Context;
using TalukdarSales.Web.Services;
using Xunit;

namespace TalukdarSales.Tests
{
    public class QueryBehaviorTests
    {
        private static void AddUser(TestApp app, string username, string first, string phone, int typeId = 1, string seq = null) =>
            app.Seed(db => db.Users.Add(new TalukdarSales.Web.Models.User
            {
                Username = username, SequencialUserId = seq ?? username, FirstName = first, LastName = "Test", PhoneNumber = phone, UserTypeId = typeId,
                Password = "", ImageName = "", Token = "", RefreshToken = "", Address = "", ContactPersonName = "", ContactPersonPhone = "", CreatedOn = DateTime.Now
            }));

        [Fact]
        public void User_search_pages_filters_and_matches_case_insensitively_in_sql()
        {
            var app = new TestApp();
            for (var i = 0; i < 30; i++) AddUser(app, "u" + i, "Person" + i, "010" + i, typeId: i % 2 == 0 ? 1 : 2);
            AddUser(app, "special", "Zainab", "999");

            var p1 = app.Run(sp => sp.GetRequiredService<UserService>().Search(null, null, 1, 12));
            var p3 = app.Run(sp => sp.GetRequiredService<UserService>().Search(null, null, 3, 12));
            Assert.Equal(31, p1.Total);
            Assert.Equal(12, p1.Items.Count);
            Assert.Equal(7, p3.Items.Count);                       // 31 = 12 + 12 + 7
            Assert.Equal(3, p3.TotalPages);

            Assert.Equal("Zainab", app.Run(sp => sp.GetRequiredService<UserService>().Search(null, "ZAINAB", 1, 12)).Items.Single().FirstName);
            Assert.Equal("Zainab", app.Run(sp => sp.GetRequiredService<UserService>().Search(null, "99", 1, 12)).Items.Single().FirstName);   // phone
            Assert.Equal(15, app.Run(sp => sp.GetRequiredService<UserService>().Search(1, "person", 1, 100)).Total);
            Assert.Equal(0, app.Run(sp => sp.GetRequiredService<UserService>().Search(null, "nobody", 1, 12)).Total);
        }

        [Fact]
        public async Task Next_user_number_follows_the_highest_issued_id_even_across_deleted_users_and_width_changes()
        {
            var app = new TestApp();
            AddUser(app, "1981-0003", "A", "1", seq: "1981-0003");
            AddUser(app, "1981-9999", "B", "2", seq: "1981-9999");
            AddUser(app, "1981-10000", "C", "3", seq: "1981-10000");
            app.Seed(db => db.Users.Single(u => u.Username == "1981-10000").IsDeleted = true);   // deleted ids still count

            var res = await app.RunAsync(sp => sp.GetRequiredService<UserService>().CreateAsync(new TalukdarSales.Web.Models.Dto.CreateUserDto { UserTypeId = 1, FirstName = "New", PhoneNumber = "5" }));
            Assert.True(res.Ok);
            Assert.Equal("1981-10001", app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().Users.OrderByDescending(u => u.Id).First().SequencialUserId));
        }

        [Fact]
        public void Requisition_page_filters_by_user_group_and_serial_in_sql()
        {
            var app = new TestApp();
            var a = app.SeedUser("a", "pw", "Ann", role: null);                      // type 1
            AddUser(app, "b", "Bob", "2", typeId: 2);
            var bId = app.Run(sp => sp.GetRequiredService<ApplicationDbContext>().Users.Single(u => u.Username == "b").Id);
            var good = app.SeedGood("Soap", 10);
            app.SeedOpenWindow();
            var svc = (Func<IServiceProvider, RequisitionService>)(sp => sp.GetRequiredService<RequisitionService>());
            app.Run(sp => svc(sp).Create(a, new[] { new RequisitionLine(good, 1) }));
            app.Run(sp => svc(sp).Create(bId, new[] { new RequisitionLine(good, 1) }));
            app.Run(sp => svc(sp).Create(bId, new[] { new RequisitionLine(good, 2) }));

            Assert.Equal(3, app.Run(sp => svc(sp).Page(true, null, null, null, null, null, 1, 25)).Total);
            var groupTwo = app.Run(sp => svc(sp).Page(true, null, 2, null, null, null, 1, 25));
            Assert.Equal(2, groupTwo.Total);
            Assert.All(groupTwo.Items, r => Assert.Equal("Bob Test", r.UserName));
            Assert.Equal(1, app.Run(sp => svc(sp).Page(true, null, null, "req - 000001", null, null, 1, 25)).Total);   // case-insensitive
            var paged = app.Run(sp => svc(sp).Page(true, null, null, null, null, null, 2, 2));
            Assert.Single(paged.Items);
            Assert.Equal(2, paged.TotalPages);
        }
    }
}
