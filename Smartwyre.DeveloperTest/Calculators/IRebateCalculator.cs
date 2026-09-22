using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Calculators
{
    public interface IRebateCalculator
    {
        IncentiveType IncentiveType { get; }
        SupportedIncentiveType RequiredProductSupport { get; }

        bool TryCalculateRebate(Rebate rebate, Product product, CalculateRebateRequest request, out decimal rebateAmount);
    }
}
