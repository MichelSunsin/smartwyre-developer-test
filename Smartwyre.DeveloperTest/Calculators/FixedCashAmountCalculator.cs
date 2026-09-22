using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Calculators
{
    public class FixedCashAmountCalculator : IRebateCalculator
    {
        public IncentiveType IncentiveType => IncentiveType.FixedCashAmount;
        public SupportedIncentiveType RequiredProductSupport => SupportedIncentiveType.FixedCashAmount;
        public bool TryCalculateRebate(Rebate rebate, Product product, CalculateRebateRequest request, out decimal rebateAmount)
        {
            rebateAmount = 0m;

            if (rebate.Amount == 0) return false;

            rebateAmount = rebate.Amount;

            return true;
        }
    }
}
