using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MvcSuperShop.Data;
using MvcSuperShop.Infrastructure.Profiles;

namespace UnitTests.TestInfrastructure.Attributes;

public class ProductServiceMoqAttribute : AutoDataAttribute
{
    public ProductServiceMoqAttribute() : base(
        () =>
        {
            var fixture = new Fixture()
            .Customize(new AutoMoqCustomization());

            fixture.Customize<Category>(x => x.Without(category => category.Id));

            var dbContext = new ApplicationDbContext(
                    new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseInMemoryDatabase(databaseName: nameof(CustomerServiceMoqDataAttribute))
                        .Options
                );

            var categories = fixture.CreateMany<Category>(fixture.Create<int>());

            dbContext.Categories!.AddRange(
                categories
            );
            dbContext.SaveChanges();

            fixture.Inject(
                dbContext
            );

            return fixture;
        }
    )
    {

    }
}
