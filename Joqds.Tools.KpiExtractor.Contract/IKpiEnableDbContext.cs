using System.Collections.Generic;

namespace Joqds.Tools.KpiExtractor.Contract
{
    public interface IKpiEnableDbContext<T> where T : class
    {
        public ISet<T> KpiValues { get; set; }
    }

    public interface IKpiEnableDbContext : IKpiEnableDbContext<KpiValue>
    {
        public ISet<KpiValue> KpiValues { get; set; }
    }
    

}
