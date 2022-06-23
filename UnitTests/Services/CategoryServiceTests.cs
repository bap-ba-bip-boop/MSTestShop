using MvcSuperShop.Data;
using MvcSuperShop.Services;
using UnitTests.TestInfrastructure.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.Linq;

namespace UnitTests.MvcSuperShop.Services;

[TestClass]
public class CategoryServiceTests
{
    [TestMethod, CustomerServiceMoqData]
    public void When_Call_GetTrendingCategories_Should_Return_Correct_Model(
        CategoryService _sut,
        int amountToTake
    )
    {
        var result = _sut.GetTrendingCategories(amountToTake);

        Assert.IsInstanceOfType(result, typeof(IEnumerable<Category>));
    }
    [TestMethod, CustomerServiceMoqData]
    public void When_Call_GetTrendingCategories_Should_Return_Correct_Amount_Of_Items(
        ApplicationDbContext dbContext,
        CategoryService _sut,
        Random _rng
    )
    {
        var amountToTake = _rng.Next(dbContext.Categories!.Count());
        var result = _sut.GetTrendingCategories(amountToTake);

        Assert.AreEqual(amountToTake, result.Count());
    }
}