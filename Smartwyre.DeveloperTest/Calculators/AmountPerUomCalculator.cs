using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Calculators
{
    public class AmountPerUomCalculator : IRebateCalculator
    {
        public IncentiveType IncentiveType => IncentiveType.AmountPerUom;
        public SupportedIncentiveType RequiredProductSupport => SupportedIncentiveType.AmountPerUom;
        public bool TryCalculateRebate(Rebate rebate, Product product, CalculateRebateRequest request, out decimal rebateAmount)
        {
            rebateAmount = 0m;

            if (rebate.Amount == 0 || request.Volume == 0)
            {
                return false;
            }

            rebateAmount = rebate.Amount * request.Volume;
            return true;
        }
    }
}
