using MvcSuperShop.Data;
using MvcSuperShop.Services;
using UnitTests.TestInfrastructure.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AutoFixture;
using System;
using AutoFixture.MSTest;
using System.Linq;

namespace UnitTests.MvcSuperShop.Services;

[TestClass]
public class CategoryServiceTests
{
    [TestMethod, AutoDomainData]
    public void When_Call_GetTrendingCategories_Should_Return_Correct_Model(
        CategoryService _sut,
        int amountToTake
    )
    {
        var result = _sut.GetTrendingCategories(amountToTake);

        Assert.IsInstanceOfType(result, typeof(IEnumerable<Category>));
    }
    [TestMethod, AutoDomainData]
    public void When_Call_GetTrendingCategories_Should_Return_Correct_Amount_Of_Items(
        Fixture fixture,
        [Frozen] ApplicationDbContext dbContext,
        CategoryService _sut,
        Random _rng,
        int amount
    )
    {
        dbContext.Categories!.AddRange(fixture.CreateMany<Category>(amount));
        dbContext.SaveChanges();

        var amountToTake = _rng.Next(amount);
        var result = _sut.GetTrendingCategories(amountToTake);

        Assert.AreEqual(amountToTake, result.Count());
    }
}