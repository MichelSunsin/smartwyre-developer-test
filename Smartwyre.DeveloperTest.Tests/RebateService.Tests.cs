using System.Collections.Generic;
using Smartwyre.DeveloperTest.Calculators;
using Smartwyre.DeveloperTest.Data.Interfaces;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests;

public class CalculatorTests
{
    [Theory]
    [InlineData(50, true, 50)]
    [InlineData(0, false, 0)]
    public void FixedCashAmount(decimal amount, bool ok, decimal expected)
    {
        var rebate = new Rebate { Amount = amount };
        var result = new FixedCashAmountCalculator()
            .TryCalculateRebate(rebate, new Product(), new CalculateRebateRequest(), out var actual);

        Assert.Equal(ok, result);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0.1, 20, 10, true, 20)]
    [InlineData(0, 20, 10, false, 0)]   // no percentage
    [InlineData(0.1, 0, 10, false, 0)]  // no price
    [InlineData(0.1, 20, 0, false, 0)]  // no volume
    public void FixedRateRebate(decimal pct, decimal price, decimal volume, bool ok, decimal expected)
    {
        var result = new FixedRateRebateCalculator().TryCalculateRebate(
            new Rebate { Percentage = pct },
            new Product { Price = price },
            new CalculateRebateRequest { Volume = volume },
            out var actual);

        Assert.Equal(ok, result);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(2, 5, true, 10)]
    [InlineData(0, 5, false, 0)]
    [InlineData(2, 0, false, 0)]
    public void AmountPerUom(decimal amount, decimal volume, bool ok, decimal expected)
    {
        var result = new AmountPerUomCalculator().TryCalculateRebate(
            new Rebate { Amount = amount },
            new Product(),
            new CalculateRebateRequest { Volume = volume },
            out var actual);

        Assert.Equal(ok, result);
        Assert.Equal(expected, actual);
    }
}

public class RebateServiceTests
{
    private class FakeStore : IRebateDataStore, IProductDataStore
    {
        public Rebate? Rebate { get; set; }
        public Product? Product { get; set; }
        public List<decimal> StoredAmounts { get; } = new();

        public Rebate? GetRebate(string id) => Rebate;
        public Product? GetProduct(string id) => Product;
        public void StoreCalculationResult(Rebate rebate, decimal amount) => StoredAmounts.Add(amount);
    }

    private static (RebateService service, FakeStore store) Create(Rebate? rebate, Product? product)
    {
        var store = new FakeStore { Rebate = rebate, Product = product };
        var service = new RebateService(store, store, new IRebateCalculator[]
        {
            new FixedCashAmountCalculator(),
            new FixedRateRebateCalculator(),
            new AmountPerUomCalculator()
        });
        return (service, store);
    }

    private static readonly CalculateRebateRequest Request = new() { RebateIdentifier = "r", ProductIdentifier = "p", Volume = 10 };

    private static Product ProductSupporting(SupportedIncentiveType t) => new() { Price = 20, SupportedIncentives = t };

    [Fact]
    public void Success_StoresCalculatedAmount()
    {
        var (service, store) = Create(
            new Rebate { Incentive = IncentiveType.FixedRateRebate, Percentage = 0.1m },
            ProductSupporting(SupportedIncentiveType.FixedRateRebate));

        var result = service.Calculate(Request);

        Assert.True(result.Success);
        Assert.Equal(new[] { 20m }, store.StoredAmounts);
    }

    [Fact]
    public void MissingRebate_Fails_AndStoresNothing()
    {
        var (service, store) = Create(null, ProductSupporting(SupportedIncentiveType.FixedCashAmount));

        Assert.False(service.Calculate(Request).Success);
        Assert.Empty(store.StoredAmounts);
    }

    [Fact]
    public void MissingProduct_Fails()
    {
        var (service, store) = Create(new Rebate { Incentive = IncentiveType.FixedCashAmount, Amount = 5 }, null);

        Assert.False(service.Calculate(Request).Success);
        Assert.Empty(store.StoredAmounts);
    }

    [Fact]
    public void ProductDoesNotSupportIncentive_Fails()
    {
        var (service, store) = Create(
            new Rebate { Incentive = IncentiveType.FixedCashAmount, Amount = 5 },
            ProductSupporting(SupportedIncentiveType.AmountPerUom));

        Assert.False(service.Calculate(Request).Success);
        Assert.Empty(store.StoredAmounts);
    }

    [Fact]
    public void InvalidCalculation_Fails()
    {
        var (service, store) = Create(
            new Rebate { Incentive = IncentiveType.FixedCashAmount, Amount = 0 },
            ProductSupporting(SupportedIncentiveType.FixedCashAmount));

        Assert.False(service.Calculate(Request).Success);
        Assert.Empty(store.StoredAmounts);
    }

    [Fact]
    public void UnregisteredIncentiveType_Fails()
    {
        var store = new FakeStore
        {
            Rebate = new Rebate { Incentive = IncentiveType.AmountPerUom, Amount = 2 },
            Product = ProductSupporting(SupportedIncentiveType.AmountPerUom)
        };
        var service = new RebateService(store, store, new IRebateCalculator[] { new FixedCashAmountCalculator() });

        Assert.False(service.Calculate(Request).Success);
    }
}