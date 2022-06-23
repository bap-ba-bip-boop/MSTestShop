using MvcSuperShop.Services;
using MvcSuperShop.Infrastructure.Context;
using Moq;
using UnitTests.TestInfrastructure.Attributes;
using AutoFixture.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AutoFixture;
using System.Linq;

namespace UnitTests.MvcSuperShop.Services;

[TestClass]
public class ProductServiceTests
{
    [TestMethod, ProductServiceMoq]
    public void When_Call_GetNewProducts_Returns_Correct_Model(
        [Frozen] Mock<IPricingService> pricingServiceMock,
        ProductService _sut,
        List<ProductServiceModel> products,
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
    [TestMethod, ProductServiceMoq]
    public void When_Call_GetNewProducts_Correct_Amount_Is_Returned(
        [Frozen] Mock<IPricingService> pricingServiceMock,
        ProductService _sut,
        CurrentCustomerContext customerContext,
        Fixture fixture,
        int amountToTake
        )
    {
        pricingServiceMock.Setup(service =>
            service.CalculatePrices(It.IsAny<IEnumerable<ProductServiceModel>>(), It.IsAny<CurrentCustomerContext>())
        ).Returns(fixture.CreateMany<ProductServiceModel>(amountToTake));

        var result = _sut.GetNewProducts(amountToTake, customerContext);

        Assert.AreEqual(amountToTake, result.Count());
    }
    [TestMethod, ProductServiceMoq]
    public void When_Call_GetNewProducts_Products_Should_Be_Sorted(
        ProductService _sut,
        CurrentCustomerContext customerContext,
        int amountToTake
        )
    {
        var isSorted = true;

        var result = _sut.GetNewProducts(amountToTake, customerContext).ToList();

        for (var i = 1; i < result.Count && isSorted; i++)
        {
            if (result[i - 1].AddedUtc < result[i].AddedUtc)
            {
                isSorted = false;
            }
        }

        Assert.IsTrue(isSorted);
    }
    //Category and manufacturer properties are included
    [TestMethod, ProductServiceMoq]
    public void When_Call_GetNewProducts_Category_And_Manufacturer_Properties_Are_Included(
        ProductService _sut,
        CurrentCustomerContext customerContext,
        int amountToTake
        )
    {
        var isNotIncluded = true;

        var result = _sut.GetNewProducts(amountToTake, customerContext).ToList();

        foreach(var item in result)
        {
            isNotIncluded = item.ManufacturerId != default && item.CategoryId != default;
        }

        Assert.IsTrue(isNotIncluded);
    }
}