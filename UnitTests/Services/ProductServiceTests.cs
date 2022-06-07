using MvcSuperShop.Services;
using MvcSuperShop.Infrastructure.Context;
using Moq;
using UnitTests.TestInfrastructure.Attributes;
using AutoFixture.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UnitTests.MvcSuperShop.Services;

[TestClass]
public class ProductServiceTests
{
    [TestMethod, AutoDomainData]
    public void When_Call_GetNewProducts_Returns_Correct_Model(
        [Frozen] Mock<IPricingService> pricingServiceMock,
        ProductService _sut,
        IEnumerable<ProductServiceModel> products,
        CurrentCustomerContext customerContext,
        int amountToTake
    )
    {
        pricingServiceMock.Setup(service =>
            service.CalculatePrices(It.IsAny<IEnumerable<ProductServiceModel>>(), It.IsAny<CurrentCustomerContext>())
        ).Returns(products);

        var result = _sut.GetNewProducts(amountToTake, customerContext);

        Assert.IsInstanceOfType(result, typeof(IEnumerable<ProductServiceModel>));
    }
}