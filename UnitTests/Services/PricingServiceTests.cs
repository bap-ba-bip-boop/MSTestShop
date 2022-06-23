using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcSuperShop.Data;
using MvcSuperShop.Infrastructure.Context;
using MvcSuperShop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitTests.TestInfrastructure.Attributes;

namespace UnitTests.MvcSuperShop.Services;

public class PricingServiceTestData
{
    public List<ProductServiceModel>? ListofProducts { get; set; }
    public int BasePrice { get; set; }
}

[TestClass]// något värde mindre än base sum????
public class PricingServiceTests
{
    [TestMethod, PricingServiceMoqData]
    public void When_Percentage_Discount_Is_Over_100_Should_Return_Base_Price(
        PricingServiceTestData testData,
        PricingService _sut
    )
    {
        var percentageDiscount = 101;
        var product = testData.ListofProducts!.First();

        var customerContext = new CurrentCustomerContext
        {
            Agreements = new List<Agreement>
            {
                new Agreement
                {
                    ValidFrom = DateTime.Now.AddHours(-1.0),
                    ValidTo = DateTime.Now.AddHours(1.0),
                    AgreementRows = new List<AgreementRow>
                    {
                        new AgreementRow
                        {
                            ManufacturerMatch = product.ManufacturerName,
                            PercentageDiscount = percentageDiscount
                        }
                    }
                }
            }
        };

        var result = _sut.CalculatePrices(testData.ListofProducts!, customerContext);

        var resultPrice = result.Select(prod => prod.Price).Sum();

        Assert.AreEqual(testData.BasePrice, resultPrice);
    }
    [TestMethod, PricingServiceMoqData]
    public void When_Percentage_Discount_Is_Negative_Should_Return_Base_Price(
        PricingServiceTestData testData,
        PricingService _sut
    )
    {
        var percentageDiscount = -1;
        var product = testData.ListofProducts!.First();

        var customerContext = new CurrentCustomerContext
        {
            Agreements = new List<Agreement>
            {
                new Agreement
                {
                    ValidFrom = DateTime.Now.AddHours(-1.0),
                    ValidTo = DateTime.Now.AddHours(1.0),
                    AgreementRows = new List<AgreementRow>
                    {
                        new AgreementRow
                        {
                            ManufacturerMatch = product.ManufacturerName,
                            PercentageDiscount = percentageDiscount
                        }
                    }
                }
            }
        };

        var result = _sut.CalculatePrices(testData.ListofProducts!, customerContext);

        var resultPrice = result.Select(prod => prod.Price).Sum();

        Assert.AreEqual(testData.BasePrice, resultPrice);
    }
    [TestMethod, PricingServiceMoqData]
    public void When_Call_CalculatePrices_Within_Valid_Time_Should_Reduce_Price(
        PricingServiceTestData testData,
        PricingService _sut,
        Random _rng
    )
    {
        var percentageDiscount = _rng.Next(1,50);

        var product = testData.ListofProducts!.First();

        var customerContext = new CurrentCustomerContext
        {
            Agreements = new List<Agreement>
            {
                new Agreement
                {
                    ValidFrom = DateTime.Now.AddHours(-1.0),
                    ValidTo = DateTime.Now.AddHours(1.0),
                    AgreementRows = new List<AgreementRow>
                    {
                        new AgreementRow
                        {
                            ProductMatch = product.Name,
                            PercentageDiscount = percentageDiscount
                        }
                    }
                }
            }
        };

        var result = _sut.CalculatePrices(testData.ListofProducts!, customerContext);

        var resultPrice = result.Select(prod => prod.Price).Sum();

        Assert.IsTrue(resultPrice < testData.BasePrice);
    }
    [TestMethod, PricingServiceMoqData]
    public void When_Call_CalculatePrices_Before_Valid_Time_Should_Not_Reduce_Price(
        PricingServiceTestData testData,
        PricingService _sut,
        Random _rng
    )
    {
        var percentageDiscount = _rng.Next(50);
        var product = testData.ListofProducts!.First();

        var customerContext = new CurrentCustomerContext
        {
            Agreements = new List<Agreement>
            {
                new Agreement
                {
                    ValidFrom = DateTime.Now.AddHours(1.0),
                    ValidTo = DateTime.Now.AddHours(2.0),
                    AgreementRows = new List<AgreementRow>
                    {
                        new AgreementRow
                        {
                            ProductMatch = product.Name,
                            PercentageDiscount = percentageDiscount
                        }
                    }
                }
            }
        };

        var result = _sut.CalculatePrices(testData.ListofProducts!, customerContext);

        var resultPrice = result.Select(prod => prod.Price).Sum();

        Assert.IsFalse(resultPrice < testData.BasePrice);
    }
    [TestMethod, PricingServiceMoqData]
    public void When_Call_CalculatePrices_After_Valid_Time_Should_Not_Reduce_Price(
        PricingServiceTestData testData,
        PricingService _sut,
        Random _rng
    )
    {
        var percentageDiscount = _rng.Next(50);
        var product = testData.ListofProducts!.First();

        var customerContext = new CurrentCustomerContext
        {
            Agreements = new List<Agreement>
            {
                new Agreement
                {
                    ValidFrom = DateTime.Now.AddHours(-2.0),//validation spacing int
                    ValidTo = DateTime.Now.AddHours(-1.0),
                    AgreementRows = new List<AgreementRow>
                    {
                        new AgreementRow
                        {
                            ProductMatch = product.Name, //List< (name, int discount)[] >
                            PercentageDiscount = percentageDiscount
                        }
                    }
                }
            }
        };

        var result = _sut.CalculatePrices(testData.ListofProducts!, customerContext);

        var resultPrice = result.Select(prod => prod.Price).Sum();

        Assert.IsFalse(resultPrice < testData.BasePrice);
    }
    [TestMethod, PricingServiceMoqData]
    public void When_Call_CalculatePrices_Should_Return_Correct_Model(
        PricingService _sut,
        IEnumerable<ProductServiceModel> products,
        CurrentCustomerContext customerContext
    )
    {
        var result = _sut.CalculatePrices(products, customerContext);

        Assert.IsInstanceOfType(result, typeof(IEnumerable<ProductServiceModel>));
    }
    [TestMethod, PricingServiceMoqData]
    public void When_Customer_Context_Is_Null_Prices_Should_Remain_Unchanged(
        PricingServiceTestData testData,
        PricingService _sut
    )
    {
        var context = default(CurrentCustomerContext);

        var result = _sut.CalculatePrices(testData.ListofProducts!, context!);

        var resultPrice = result.Select(prod => prod.Price).Sum();

        Assert.AreEqual(testData.BasePrice, resultPrice);
    }
    [TestMethod, PricingServiceMoqData]
    public void When_Call_CalculatePrices_With_Two_Discounts_Should_Pick_The_Lowest(
        PricingServiceTestData testData,
        PricingService _sut
    )
    {
        var product = testData.ListofProducts!.First();// kör en calc för den första discounten, en för den andra och jämför priserna

        var lowerDiscount = 10;
        var higherDiscount = 20;

        var Agreement1 = new Agreement
        {
            ValidFrom = DateTime.Now.AddHours(-1.0),
            ValidTo = DateTime.Now.AddHours(1.0),
            AgreementRows = new List<AgreementRow>
            {
                new AgreementRow
                {
                    ProductMatch = product.Name,
                    PercentageDiscount = lowerDiscount
                }
            }
        };
        var Agreement2 = new Agreement
        {
            ValidFrom = DateTime.Now.AddHours(-1.0),
            ValidTo = DateTime.Now.AddHours(1.0),
            AgreementRows = new List<AgreementRow>
            {
                new AgreementRow
                {
                    ProductMatch = product.Name,
                    PercentageDiscount = higherDiscount
                }
            }
        };

        var context1 = new CurrentCustomerContext()
        {
            Agreements = new List<Agreement>
            {
                Agreement1
            }
        };
        var context2 = new CurrentCustomerContext()
        {
            Agreements = new List<Agreement>
            {
                Agreement2
            }
        };
        var context3 = new CurrentCustomerContext
        {
            Agreements = new List<Agreement>
            {
                Agreement1,
                Agreement2
            }
        };

        var lowerTotal = _sut.CalculatePrices(testData.ListofProducts!, context2).Select(prod => prod.Price).Sum();
        var HigherTotal = _sut.CalculatePrices(testData.ListofProducts!, context1).Select(prod => prod.Price).Sum();

        var result = _sut.CalculatePrices(testData.ListofProducts!, context3);

        var recievedSum = result.Select(product => product.Price).Sum();

        Assert.AreEqual(lowerTotal, recievedSum);
    }
    [TestMethod, PricingServiceMoqData]
    public void When_Call_CalculatePrices_Should_Return_Correct_Price(
        PricingService _sut
    )
    {
        var listofProducts = new List<ProductServiceModel>();
        listofProducts!.Add(
            new ProductServiceModel
            {
                Name = "Laptop",
                BasePrice = 200
            }
        );
        listofProducts!.Add(
            new ProductServiceModel
            {
                Name = "Video Game",
                BasePrice = 50
            }
        );
        listofProducts!.Add(
            new ProductServiceModel
            {
                Name = "Milk",
                BasePrice = 10
            }
        );

        var customerContext = new CurrentCustomerContext
        {
            Agreements = new List<Agreement>
            {
                new Agreement
                {
                    ValidFrom = DateTime.Now.AddHours(-1.0),
                    ValidTo = DateTime.Now.AddHours(1.0),
                    AgreementRows = new List<AgreementRow>
                    {
                        new AgreementRow
                        {
                            ProductMatch = "Milk",
                            PercentageDiscount = 10
                        }
                    }
                },
                new Agreement
                {
                    ValidFrom = DateTime.Now.AddHours(-1.0),
                    ValidTo = DateTime.Now.AddHours(1.0),
                    AgreementRows = new List<AgreementRow>
                    {
                        new AgreementRow
                        {
                            ProductMatch = "Laptop",
                            PercentageDiscount = 20
                        },
                        new AgreementRow
                        {
                            ProductMatch = "Video Game",
                            PercentageDiscount = 10
                        }
                    }
                },
                new Agreement
                {
                    ValidFrom = DateTime.Now.AddHours(-1.0),
                    ValidTo = DateTime.Now.AddHours(1.0),
                    AgreementRows = new List<AgreementRow>
                    {
                        new AgreementRow
                        {
                            ProductMatch = "Laptop",
                            PercentageDiscount = 5
                        },
                        new AgreementRow
                        {
                            ProductMatch = "Video Game",
                            PercentageDiscount = 5
                        }
                    }
                },
            }
        };
        //260 214

        var expectedSum = 214;

        var result = _sut.CalculatePrices(listofProducts!, customerContext);

        var recievedSum = result.Select(product => product.Price).Sum();

        Assert.AreEqual(expectedSum, recievedSum);
    }
}
