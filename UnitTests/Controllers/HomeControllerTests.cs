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
    [TestMethod, HomeControllerMoqAutoData]
    public void When_Should_Only_Recieve_Deals_For_Logged_In_Customer(
        Fixture fixture,
        ClaimsPrincipal user,
        ApplicationDbContext dbContext,

        CategoryService categoryService,
        ProductService productService,
        IMapper mapper,
        IOptions<HomeControllerSettings> options,
        ControllerContext controllerContext
        )
    {
        var productName = fixture.Create<string>();
        //retrieve email and userId
        user.Claims.GetEnumerator();
        var userInfo = user.Identities.First().Claims.Select(uInfo => uInfo.Value).ToList();
        var Email = userInfo[1];
        var UserId = Guid.Parse(userInfo[0]);
        //create CustomerContext with correct email and userId
        var UserAgreement = new UserAgreements
        {
            Email = userInfo[1],
            Agreement = new Agreement
            {
                ValidFrom = DateTime.Now.AddDays(-1),
                ValidTo = DateTime.Now.AddDays(1),
                AgreementRows = new List<AgreementRow>
                {
                    new AgreementRow
                    {
                        ProductMatch = productName,
                        PercentageDiscount = 50
                    }
                }
            }
        };
        dbContext.UserAgreements!.Add(UserAgreement);
        dbContext.SaveChanges();
        
        //add products that correlate to the CustomerContext
        dbContext.Products!.Add(
            new Product
            {
                Name = productName,
                BasePrice = 100
            }
        );
        dbContext.SaveChanges();

        var _sut = new HomeController(categoryService, productService, mapper, dbContext, options);
        _sut.ControllerContext = controllerContext;

        var expectedPrice = 50;

        var result = _sut!.Index() as ViewResult;
        var resultModel = result!.Model as HomeIndexViewModel;

        var resultTotal = resultModel!.NewProducts!.Select(prod => prod.Price).Sum();

        Assert.AreEqual(expectedPrice, resultTotal);
    }
}
