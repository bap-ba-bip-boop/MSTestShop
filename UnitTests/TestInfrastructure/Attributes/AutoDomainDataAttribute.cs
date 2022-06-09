using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using Microsoft.EntityFrameworkCore;
using MvcSuperShop.Data;

namespace UnitTests.TestInfrastructure.Attributes;

public class AutoDomainDataAttribute : AutoDataAttribute
{
    public AutoDomainDataAttribute() : base(
        () =>
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization() { ConfigureMembers = false });

            fixture.Inject(
                new ApplicationDbContext(
                    new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseInMemoryDatabase(databaseName: "AutoDomain Database")
                        .Options
                )
            );

            return fixture;
        }
    )
    {

    }
}