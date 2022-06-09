using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvcSuperShop.Data;
using MvcSuperShop.Infrastructure.Context;
using MvcSuperShop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitTests.TestInfrastructure.Attributes;

namespace UnitTests.MvcSuperShop.Services;

[TestClass]
public class PricingServiceTests
{
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
        int basePrice,
        Random _rng
    )
    {
        var percentageDiscount = _rng.Next(50);
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

        var expectedResult = basePrice * (1 - percentageDiscount / 100m);

        var result = _sut.CalculatePrices(listofProducts, customerContext);

        var resultPrice = result.Select(prod => prod.Price).Sum();

        Assert.AreEqual(Convert.ToInt32(Math.Round(expectedResult, 0)), resultPrice);
    }
    [TestMethod, AutoDomainData]
    public void When_Call_CalculatePrices_Before_Valid_Time_Should_Not_Reduce_Price(
        PricingService _sut,
        DateTime endDate,
        int basePrice,
        Random _rng
    )
    {
        var percentageDiscount = _rng.Next(50);
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
                    ValidFrom = DateTime.Now.AddHours(1.0),
                    ValidTo = DateTime.Now.AddHours(2.0),
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
    public void When_Call_CalculatePrices_After_Valid_Time_Should_Not_Reduce_Price(
        PricingService _sut,
        DateTime endDate,
        int basePrice,
        Random _rng
    )
    {
        var percentageDiscount = _rng.Next(50);
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
                    ValidFrom = DateTime.Now.AddHours(-2.0),
                    ValidTo = DateTime.Now.AddHours(-1.0),
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
        IEnumerable<ProductServiceModel> products,
        PricingService _sut
    )
    {
        var context = default(CurrentCustomerContext);

        var expectedPrice = products.Select(prod => prod.BasePrice).Sum();

        var result = _sut.CalculatePrices(products, context!);

        var resultPrice = result.Select(prod => prod.BasePrice).Sum();

        Assert.AreEqual(expectedPrice, resultPrice);
    }
    [TestMethod, AutoDomainData]
    public void When_Call_CalculatePrices_With_Two_Discounts_Should_Pick_The_Lowest(
        PricingService _sut
    )
    {
        var listofProducts = new List<ProductServiceModel>
        {
            new ProductServiceModel
            {
                Name = "Laptop",
                BasePrice = 200
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
                            PercentageDiscount = 20
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
                            PercentageDiscount = 10
                        }
                    }
                },
            }
        };

        var expectedSum = 160;

        var result = _sut.CalculatePrices(listofProducts, customerContext);

        var recievedSum = result.Select(product => product.Price).Sum();

        Assert.AreEqual(expectedSum, recievedSum);
    }
    [TestMethod, AutoDomainData]
    public void When_Call_CalculatePrices_Should_Return_Correct_Price(
        PricingService _sut
    )
    {
        var listofProducts = new List<ProductServiceModel>
        {
            new ProductServiceModel
            {
                Name = "Laptop",
                BasePrice = 200
            },
            new ProductServiceModel
            {
                Name = "Video Game",
                BasePrice = 50
            },
            new ProductServiceModel
            {
                Name = "Milk",
                BasePrice = 10
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
                            CategoryMatch = "Milk",
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
                            PercentageDiscount = 20
                        },
                        new AgreementRow
                        {
                            CategoryMatch = "Video Game",
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
        //200 160
        // 50  45
        // 10   9
        //260 214

        var expectedSum = 214;

        var result = _sut.CalculatePrices(listofProducts, customerContext);

        var recievedSum = result.Select(product => product.Price).Sum();

        Assert.AreEqual(expectedSum, recievedSum);
    }
}
