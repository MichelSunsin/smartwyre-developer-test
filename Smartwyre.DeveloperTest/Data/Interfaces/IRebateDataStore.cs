using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Data.Interfaces
{
    public interface IRebateDataStore
    {
        Rebate? GetRebate(string rebateIdentifier);
        void StoreCalculationResult(Rebate account, decimal rebateAmount);
    }
}
