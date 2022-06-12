using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MvcSuperShop.Controllers;
using MvcSuperShop.Data;
using MvcSuperShop.Infrastructure.Context;
using MvcSuperShop.Services;
using MvcSuperShop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UnitTests.TestInfrastructure.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AutoFixture.MSTest;
using AutoFixture;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Controllers;

//result.ViewNAme == null
//verifiera att GetNewProducts blir anropad med korrekta värden och med korrekt customercontext

[TestClass]
public class HomeControllerTests
{
    private HomeController? _sut;

    [TestInitialize, AutoDomainData]
    public void Initialize(
        [Frozen] Mock<ICategoryService> _categoryServiceMock,
        [Frozen] Mock<IProductService> _productServiceMock,
        [Frozen] Mock<IMapper> _mapper,
        Fixture fixture)
    {
        _sut = new HomeController(_categoryServiceMock.Object, _productServiceMock.Object, _mapper.Object, fixture.Create<ApplicationDbContext>());
    }

    [TestMethod, AutoDomainData]
    public void When_Call_Index_Should_Return_ViewModel(
        [Frozen] Mock<ICategoryService> _categoryServiceMock,
        [Frozen] Mock<IProductService> _productServiceMock,
        [Frozen] Mock<IMapper> _mapper,
        IEnumerable<Category> categories,
        IEnumerable<ProductServiceModel> products,
        List<ProductBoxViewModel> mapperProductLsit,
        List<CategoryViewModel> mapperCategoryList
    )
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "gunnar@somecompany.com")
        }, "TestAuthentication"));


        _sut!.ControllerContext = new ControllerContext();
        _sut.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = user
        };

        _categoryServiceMock!.Setup( cService => cService.GetTrendingCategories(It.IsAny<int>())).Returns(categories);
        _productServiceMock!.Setup(pService => pService.GetNewProducts(It.IsAny<int>(), It.IsAny<CurrentCustomerContext>())).Returns(products);

        _mapper!.Setup(mapper => mapper.Map<List<CategoryViewModel>>(categories)).Returns(mapperCategoryList);
        _mapper.Setup(mapper => mapper.Map<List<ProductBoxViewModel>>(products)).Returns(mapperProductLsit);

        var result = _sut!.Index() as ViewResult;
        var resultModel = result!.Model as HomeIndexViewModel;

        Assert.AreEqual(resultModel!.NewProducts, mapperProductLsit);
        Assert.AreEqual(resultModel!.TrendingCategories, mapperCategoryList);
    }
}
