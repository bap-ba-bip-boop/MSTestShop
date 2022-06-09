using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcSuperShop.Data;
using MvcSuperShop.Infrastructure.Context;
using MvcSuperShop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitTests.TestInfrastructure.Attributes;

namespace UnitTests.MvcSuperShop.Services;

[TestClass]// något värde mindre än base sum????
public class PricingServiceTests
{
    private List<ProductServiceModel>? listofProducts;
    private int basePrice;

    [TestInitialize]
    public void Initialize()
    {
        var fixture = new Fixture();

        listofProducts = new List<ProductServiceModel>
        {
            new ProductServiceModel
            {
                Name = fixture.Create<string>(),
                BasePrice = fixture.Create<int>()
            },
            new ProductServiceModel
            {
                Name = fixture.Create<string>(),
                BasePrice = fixture.Create<int>()
            },
            new ProductServiceModel
            {
                Name = fixture.Create<string>(),
                BasePrice = fixture.Create<int>()
            }
        };

        basePrice = listofProducts.Select(prod => prod.BasePrice).Sum();
    }

    [TestMethod, AutoDomainData]
    public void When_Percentage_Discount_Is_Over_100_Should_Return_Base_Price(
        PricingService _sut,
        int basePrice,
        Random _rng
    )
    {
        var percentageDiscount = 101;
        var listofProducts = new List<ProductServiceModel>
        {
            new ProductServiceModel
            {
                Name = "Laptop",
                BasePrice = basePrice
            }
        };

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
                            ManufacturerMatch = "Laptop",
                            PercentageDiscount = percentageDiscount
                        }
                    }
                }
            }
        };

        var expectedResult = basePrice;

        var result = _sut.CalculatePrices(listofProducts, customerContext);

        var resultPrice = result.Select(prod => prod.Price).Sum();

        Assert.AreEqual(expectedResult, resultPrice);
    }
    [TestMethod, AutoDomainData]
    public void When_Percentage_Discount_Is_Negative_Should_Return_Base_Price(
        PricingService _sut,
        int basePrice,
        Random _rng
    )
    {
        var percentageDiscount = -1;
        var listofProducts = new List<ProductServiceModel>
        {
            new ProductServiceModel
            {
                Name = "Laptop",
                BasePrice = basePrice
            }
        };

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
                            ManufacturerMatch = "Laptop",
                            PercentageDiscount = percentageDiscount
                        }
                    }
                }
            }
        };

        var expectedResult = basePrice;

        var result = _sut.CalculatePrices(listofProducts, customerContext);

        var resultPrice = result.Select(prod => prod.Price).Sum();

        Assert.AreEqual(expectedResult, resultPrice);
    }
    [TestMethod, AutoDomainData]
    public void When_Call_CalculatePrices_Within_Valid_Time_Should_Reduce_Price(
        PricingService _sut,
        Random _rng
    )
    {
        var percentageDiscount = _rng.Next(50);

        var product = listofProducts!.First();

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

        var result = _sut.CalculatePrices(listofProducts!, customerContext);

        var resultPrice = result.Select(prod => prod.Price).Sum();

        Assert.IsTrue(resultPrice < basePrice);
    }
    [TestMethod, AutoDomainData]
    public void When_Call_CalculatePrices_Before_Valid_Time_Should_Not_Reduce_Price(
        PricingService _sut,
        Random _rng
    )
    {
        var percentageDiscount = _rng.Next(50);
        var product = listofProducts!.First();

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

        var result = _sut.CalculatePrices(listofProducts!, customerContext);

        var resultPrice = result.Select(prod => prod.Price).Sum();

        Assert.IsFalse(resultPrice < basePrice);
    }
    [TestMethod, AutoDomainData]
    public void When_Call_CalculatePrices_After_Valid_Time_Should_Not_Reduce_Price(
        PricingService _sut,
        Random _rng
    )
    {
        var percentageDiscount = _rng.Next(50);
        var product = listofProducts!.First();

        var customerContext = new CurrentCustomerContext
        {
            Agreements = new List<Agreement>
            {
                new Agreement
                {
                    ValidFrom = DateTime.Now.AddHours(-2.0),
                    ValidTo = DateTime.Now.AddHours(-1.0),
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

        var result = _sut.CalculatePrices(listofProducts!, customerContext);

        var resultPrice = result.Select(prod => prod.Price).Sum();

        Assert.IsFalse(resultPrice < basePrice);
    }
    [TestMethod, AutoDomainData]
    public void When_Call_CalculatePrices_Should_Return_Correct_Model(
        PricingService _sut,
        IEnumerable<ProductServiceModel> products,
        CurrentCustomerContext customerContext
    )
    {
        var result = _sut.CalculatePrices(products, customerContext);

        Assert.IsInstanceOfType(result, typeof(IEnumerable<ProductServiceModel>));
    }
    [TestMethod, AutoDomainData]
    public void When_Customer_Context_Is_Null_Prices_Should_Remain_Unchanged(
        PricingService _sut
    )
    {
        var context = default(CurrentCustomerContext);

        var result = _sut.CalculatePrices(listofProducts!, context!);

        var resultPrice = result.Select(prod => prod.Price).Sum();

        Assert.AreEqual(basePrice, resultPrice);
    }
    [TestMethod, AutoDomainData]
    public void When_Call_CalculatePrices_With_Two_Discounts_Should_Pick_The_Lowest(
        PricingService _sut
    )
    {
        var product = listofProducts!.First();// kör en calc för den första discounten, en för den andra och jämför priserna

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

        var lowerTotal = _sut.CalculatePrices(listofProducts!, context2).Select(prod => prod.Price).Sum();
        var HigherTotal = _sut.CalculatePrices(listofProducts!, context1).Select(prod => prod.Price).Sum();

        var result = _sut.CalculatePrices(listofProducts!, context3);

        var recievedSum = result.Select(product => product.Price).Sum();

        Assert.AreEqual(lowerTotal, recievedSum);
    }
    [TestMethod, AutoDomainData]
    public void When_Call_CalculatePrices_Should_Return_Correct_Price(
        PricingService _sut
    )
    {
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
                            ManufacturerMatch = "Laptop",
                            PercentageDiscount = 5
                        },
                        new AgreementRow
                        {
                            CategoryMatch = "Video Game",
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
