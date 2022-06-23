using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.MSTest;
using MvcSuperShop.Services;
using System.Linq;
using UnitTests.MvcSuperShop.Services;

namespace UnitTests.TestInfrastructure.Attributes;

public class PricingServiceMoqDataAttribute : AutoDataAttribute
{
    public PricingServiceMoqDataAttribute() : base(
        () =>
        {
            var fixture = new Fixture()
            .Customize(new AutoMoqCustomization());

            var psTD = new PricingServiceTestData
            {
                ListofProducts = new()
            };
            var listofprodServiceModel = fixture.CreateMany<ProductServiceModel>();

            fixture.Customize<PricingServiceTestData>(
                ob => ob.With(pstd => pstd.ListofProducts!,listofprodServiceModel.ToList())
                .With(pstd => pstd.BasePrice, listofprodServiceModel.Select(prod => prod.BasePrice).Sum())
            );

            return fixture;
        }
    )
    {

    }
}