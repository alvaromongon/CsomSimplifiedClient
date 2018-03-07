using System.Threading.Tasks;
using Microsoft.ProjectServer.Client;

namespace Csom.Client.Extensions
{
    public static class EnterpriseResourceExtensions
    {
        public static async Task<EnterpriseResourceCostRateTable> GetDefaultCostRateTableAsync(this EnterpriseResource enterpriseResource)
        {
            enterpriseResource.Context.Load(enterpriseResource.CostRateTables);
            await enterpriseResource.Context.ExecuteQueryAsync();

            var costRateTable = enterpriseResource.CostRateTables.GetByName(CostRateTableName.A);
            enterpriseResource.Context.Load(costRateTable.CostRates);
            await enterpriseResource.Context.ExecuteQueryAsync();

            return costRateTable;
        }
    }
}
