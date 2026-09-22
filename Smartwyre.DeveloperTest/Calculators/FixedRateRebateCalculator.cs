using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Calculators
{
    public class FixedRateRebateCalculator : IRebateCalculator
    {
        public IncentiveType IncentiveType => IncentiveType.FixedRateRebate;
        public SupportedIncentiveType RequiredProductSupport => SupportedIncentiveType.FixedRateRebate;
        public bool TryCalculateRebate(Rebate rebate, Product product, CalculateRebateRequest request, out decimal rebateAmount)
        {
            rebateAmount = 0m;
            
            if (rebate.Percentage == 0 || product.Price == 0 || request.Volume == 0)
            {
                return false;
            }

            rebateAmount = product.Price * rebate.Percentage * request.Volume;
            return true;
        }
    }
}
