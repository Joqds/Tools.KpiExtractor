using System.Collections.Generic;

namespace Joqds.Tools.KpiExtractor.Contract
{
    public class KpiType
    {
        public int Id { get; set; }
        public DatePart DatePart { get; set; }
        public ExtractionType ExtractionType { get; set; }
        public int? CleanUpAfter { get; set; }
        public string Query { get; set; }
        public KpiTypeStatus Status { get; set; }
        public virtual ICollection<KpiParameter> Parameters { get; set; }
        public virtual ICollection<KpiValue> KpiValues { get; set; }

        //todo: add default value
    }
}