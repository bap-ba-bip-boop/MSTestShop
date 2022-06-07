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
    //if percentageDiscount is over 100 should return Base Price?
    //if percentageDiscount is negative should return Base Price?
    //if Agreement is out of date should not apply discount
    //-- if it before the agreement is valid it should not count
    //-- if it is after the agreement is valid it should not count
    //if the agreement is during the valid time it should count
    [TestMethod, AutoDomainData]
    public void When_Call_CalculatePrices_Within_Valid_Time_Should_Reduce_Price(
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
                    ValidFrom = endDate.AddHours(-1.0),
                    ValidTo = endDate.AddHours(1.0),
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
                    ValidFrom = endDate.AddHours(1.0),
                    ValidTo = endDate.AddHours(2.0),
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
                    ValidFrom = endDate.AddHours(-2.0),
                    ValidTo = endDate.AddHours(-1.0),
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

        //if it has a choice between 2 different discount it will pick the bigger one

        //200 160
        // 50  45
        // 10   9
        //260 214

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

        //if it has a choice between 2 different discount it will pick the bigger one

        //200 160
        // 50  45
        // 10   9
        //260 214

        var expectedSum = 214;

        var result = _sut.CalculatePrices(listofProducts, customerContext);

        var recievedSum = result.Select(product => product.Price).Sum();

        Assert.AreEqual(expectedSum, recievedSum);
    }

    private static AgreementRow CreateAgreementRow(Random _rng, IEnumerable<ProductServiceModel> products)
    {
        var newAgreementRow = new AgreementRow();
        var AgreementType = _rng.Next(1, 3);

        var agreementToApply = products.ElementAt(_rng.Next(products.Count()));

        if (AgreementType == 1)
        {
            newAgreementRow.ManufacturerMatch = agreementToApply.Name;
        }
        else if (AgreementType == 2)
        {
            newAgreementRow.ProductMatch = agreementToApply.Name;
        }
        else if (AgreementType == 3)
        {
            newAgreementRow.CategoryMatch = agreementToApply.Name;
        }

        var maxDouble = 50.0;
        var minDouble = 10.0;

        var percentageDiscount = (decimal)(_rng.NextDouble()*(maxDouble-minDouble) + minDouble);

        newAgreementRow.PercentageDiscount = percentageDiscount;

        return newAgreementRow;
    }
    private static Agreement CreateAgreement(Random _rng, IEnumerable<ProductServiceModel> products)
    {
        //Create Agreement
        var Agreement = new Agreement();

        var AgreementRowAmount = _rng.Next(1, 5);

        //CreateAgreeMentRow
        for (int i = 0; i < AgreementRowAmount; i++)
        {
            Agreement.AgreementRows.Add(CreateAgreementRow(_rng, products));
        }

        return Agreement;
    }
    private static IEnumerable<Agreement> getCustomerContextAgreements(int amount, Random _rng, IEnumerable<ProductServiceModel> products)
    {
        for (int i = 0; i < amount; i++)
        {
            yield return CreateAgreement(_rng, products);
        }
    }
    private static int calculateExpectedPrice(IEnumerable<ProductServiceModel> products, CurrentCustomerContext customerContext)
    {
        //for each product
        foreach (var prod in products)
        {
            var lowest = prod.BasePrice;
            //we check each Agreement in Customer Context
            foreach (var agreement in customerContext.Agreements!)
            {
                //we check each of the agreement types
                foreach (var row in agreement.AgreementRows)
                {
                    //if there are any deals for the product for the specific type
                    var check = false;
                    if (!string.IsNullOrEmpty(row.ManufacturerMatch) && row.ManufacturerMatch.ToLower().Contains(prod.Name!.ToLower()))
                    {
                        check = true;
                    }
                    else if (!string.IsNullOrEmpty(row.ProductMatch) && row.ProductMatch.ToLower().Contains(prod.Name!.ToLower()))
                    {
                        check = true;
                    }
                    else if (!string.IsNullOrEmpty(row.CategoryMatch) && row.CategoryMatch.ToLower().Contains(prod.Name!.ToLower()))
                    {
                        check = true;
                    }

                    if (check)
                    {
                        var temp = (1m - row.PercentageDiscount / 100m) * prod.Price;

                        if (temp < lowest)
                            lowest = decimal.ToInt32(temp);
                    }
                }
            }
            prod.Price = lowest;
        }

        return products.Select(product => product.Price).Sum();
    }
}
