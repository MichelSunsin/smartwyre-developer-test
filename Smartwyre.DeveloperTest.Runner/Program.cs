using Microsoft.Extensions.DependencyInjection;
using Smartwyre.DeveloperTest.Calculators;
using Smartwyre.DeveloperTest.Data.Interfaces;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Smartwyre.DeveloperTest.Runner;

class Program
{
    static int Main(string[] args)
    {
        var services = new ServiceCollection();

        var calculatorTypes = typeof(IRebateCalculator).Assembly.GetTypes()
            .Where(t => typeof(IRebateCalculator).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var calculatorType in calculatorTypes)
            services.AddSingleton(typeof(IRebateCalculator), calculatorType);

        var store = new InMemoryStore();
        services.AddSingleton<IRebateDataStore>(store);
        services.AddSingleton<IProductDataStore>(store);
        services.AddSingleton<IRebateService, RebateService>();

        var service = services.BuildServiceProvider().GetRequiredService<IRebateService>();

        string rebateId, productId;
        decimal volume;

        Console.WriteLine("Available rebates: FIXED, RATE, UOM");
        Console.WriteLine("Available products: P1, P2, P3");
        rebateId = Prompt("Rebate identifier: ");
        productId = Prompt("Product identifier: ");
        while (!decimal.TryParse(Prompt("Volume: "), NumberStyles.Number, CultureInfo.InvariantCulture, out volume))
            Console.WriteLine("Please enter a valid number.");

        var result = service.Calculate(new CalculateRebateRequest
        {
            RebateIdentifier = rebateId,
            ProductIdentifier = productId,
            Volume = volume
        });

        Console.WriteLine($"Success: {result.Success}");
        if (result.Success)
            Console.WriteLine($"Stored amount: {store.LastStoredAmount}");

        return result.Success ? 0 : 1;
    }

    static string Prompt(string text)
    {
        Console.Write(text);
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }
}

internal class InMemoryStore : IRebateDataStore, IProductDataStore
{
    private readonly Dictionary<string, Rebate> _rebates = new()
    {
        ["FIXED"] = new Rebate { Identifier = "FIXED", Incentive = IncentiveType.FixedCashAmount, Amount = 10m },
        ["RATE"] = new Rebate { Identifier = "RATE", Incentive = IncentiveType.FixedRateRebate, Percentage = 0.1m },
        ["UOM"] = new Rebate { Identifier = "UOM", Incentive = IncentiveType.AmountPerUom, Amount = 1m },
    };

    private readonly Dictionary<string, Product> _products = new()
    {
        ["P1"] = new Product
        {
            Id = 1,
            Identifier = "P1",
            Price = 10m,
            Uom = "kg",
            SupportedIncentives = SupportedIncentiveType.FixedCashAmount
                                | SupportedIncentiveType.FixedRateRebate
                                | SupportedIncentiveType.AmountPerUom
        },
        ["P2"] = new Product
        {
            Id = 2,
            Identifier = "P2",
            Price = 20m,
            Uom = "kg",
            SupportedIncentives = SupportedIncentiveType.FixedRateRebate 
                                | SupportedIncentiveType.FixedRateRebate
        },
        ["P3"] = new Product
        {
            Id = 3,
            Identifier = "P3",
            Price = 30m,
            Uom = "kg",
            SupportedIncentives = SupportedIncentiveType.AmountPerUom
        }
    };

    public decimal? LastStoredAmount { get; private set; }
    public Rebate? GetRebate(string id) => _rebates.GetValueOrDefault(id);
    public Product? GetProduct(string id) => _products.GetValueOrDefault(id);
    public void StoreCalculationResult(Rebate rebate, decimal amount) => LastStoredAmount = amount;
}
