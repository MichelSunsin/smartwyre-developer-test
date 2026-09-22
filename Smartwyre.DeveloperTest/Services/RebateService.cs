using Smartwyre.DeveloperTest.Calculators;
using Smartwyre.DeveloperTest.Data.Interfaces;
using Smartwyre.DeveloperTest.Types;
using System.Collections.Generic;
using System.Linq;

namespace Smartwyre.DeveloperTest.Services;

public class RebateService : IRebateService
{
    private readonly IRebateDataStore _rebateDataStore;
    private readonly IProductDataStore _productDataStore;
    private readonly IReadOnlyDictionary<IncentiveType, IRebateCalculator> _rebateCalculators;

    public RebateService(IRebateDataStore rebateDataStore, IProductDataStore productDataStore, IEnumerable<IRebateCalculator> rebateCalculators)
    {
        _rebateDataStore = rebateDataStore;
        _productDataStore = productDataStore;
        _rebateCalculators = rebateCalculators.ToDictionary(x => x.IncentiveType);
    }

    public CalculateRebateResult Calculate(CalculateRebateRequest request)
    {
        Rebate? rebate = _rebateDataStore.GetRebate(request.RebateIdentifier);
        Product? product = _productDataStore.GetProduct(request.ProductIdentifier);

        if (rebate is null || product is null) return Failure();

        if (!_rebateCalculators.TryGetValue(rebate.Incentive, out var calculator)) return Failure();

        if (!product.SupportedIncentives.HasFlag(calculator.RequiredProductSupport)) return Failure();

        if (!calculator.TryCalculateRebate(rebate, product, request, out var rebateAmount)) return Failure();

        _rebateDataStore.StoreCalculationResult(rebate, rebateAmount);

        return new CalculateRebateResult { Success = true };
    }

    private static CalculateRebateResult Failure()
    {
        return new CalculateRebateResult { Success = false };
    }
}
