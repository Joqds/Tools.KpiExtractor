using System;

namespace Joqds.Tools.KpiExtractor.Contract
{
    public class KpiValue
    {
        public long Id { get; set; }
        public string TargetId { get; set; }
        public int KpiTypeId { get; set; }
        public decimal Value { get; set; }
        public DateTime DateTimePoint { get; set; }
        public DateTime ExtractedDateTime { get; set; }

        public virtual KpiType KpiType { get; set; }
    }

   
}