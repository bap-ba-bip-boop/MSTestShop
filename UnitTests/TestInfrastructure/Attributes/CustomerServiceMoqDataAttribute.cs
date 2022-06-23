using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using Microsoft.EntityFrameworkCore;
using MvcSuperShop.Data;

namespace UnitTests.TestInfrastructure.Attributes;

public class CustomerServiceMoqDataAttribute : AutoDataAttribute
{
    public CustomerServiceMoqDataAttribute() : base(
        () =>
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            fixture.Customize<Category>( x => x.Without(category => category.Id));

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