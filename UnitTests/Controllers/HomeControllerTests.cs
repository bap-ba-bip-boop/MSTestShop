using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MvcSuperShop.Controllers;
using MvcSuperShop.Data;
using MvcSuperShop.Infrastructure.Context;
using MvcSuperShop.Services;
using MvcSuperShop.ViewModels;
using System.Collections.Generic;
using UnitTests.TestInfrastructure.Attributes;
using Microsoft.AspNetCore.Mvc;
using AutoFixture;
using Microsoft.Extensions.Options;
using MSTestShop.Settings;
using System.Security.Claims;
using System;
using System.Linq;
using AutoMapper;

namespace UnitTests.Controllers;

[TestClass]
public class HomeControllerTests
{
    [TestMethod, HomeControllerMoqAutoData]
    public void When_Call_Index_Should_Return_ViewModel(
        Mock<ICategoryService> _categoryServiceMock,
        Mock<IProductService> _productServiceMock,
        HomeController _sut,
        IEnumerable<Category> categories,
        IEnumerable<ProductServiceModel> products
    )
    {
        _categoryServiceMock!.Setup( cService => cService.GetTrendingCategories(It.IsAny<int>())).Returns(categories);
        _productServiceMock!.Setup(pService => pService.GetNewProducts(It.IsAny<int>(), It.IsAny<CurrentCustomerContext>())).Returns(products);

        var result = _sut!.Index() as ViewResult;
        var resultModel = result!.Model as HomeIndexViewModel;

        Assert.IsInstanceOfType(resultModel!.TrendingCategories, typeof(List<CategoryViewModel>) );
        Assert.IsInstanceOfType(resultModel!.NewProducts, typeof(List<ProductBoxViewModel>) );
    }
    [TestMethod, HomeControllerMoqAutoData]
    public void When_Call_Index_Should_Return_Correct_Amounts_of_items(
        Mock<ICategoryService> _categoryServiceMock,
        Mock<IProductService> _productServiceMock,
        IOptions<HomeControllerSettings> _options,
        HomeController _sut,

        Fixture fixture
    )
    {
        var prodAmount = _options!.Value.ProductAmount;
        var cateAmount = _options.Value.TrendigCategoriesAmount;

        _categoryServiceMock!.Setup(cService => cService.GetTrendingCategories(It.IsAny<int>())).Returns(
            fixture.CreateMany<Category>(cateAmount)
        );
        _productServiceMock!.Setup(pService => pService.GetNewProducts(It.IsAny<int>(), It.IsAny<CurrentCustomerContext>())).Returns(
            fixture.CreateMany<ProductServiceModel>(prodAmount)
        );

        var result = _sut!.Index() as ViewResult;
        var resultModel = result!.Model as HomeIndexViewModel;

        Assert.AreEqual(resultModel!.NewProducts!.Count, prodAmount);
        Assert.AreEqual(resultModel!.TrendingCategories!.Count, cateAmount);
    }
}
