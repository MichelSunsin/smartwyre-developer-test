using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Data.Interfaces
{
    public interface IProductDataStore
    {
        Product? GetProduct(string productIdentifier);
    }
}
