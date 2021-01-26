using Joqds.Tools.KpiExtractor.Contract;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Joqds.Tools.KpiExtractor.Core.Repository
{
    public interface IKpiSeeder
    {
        public Task<IEnumerable<CreateKpiDto<KpiValue>>> Seed();
    }
}