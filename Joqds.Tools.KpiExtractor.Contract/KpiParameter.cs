using System.Data;

namespace Joqds.Tools.KpiExtractor.Contract
{
    public class KpiParameter
    {
        public int Id { get; set; }
        public int KpiId { get; set; }
        public SqlDbType DbType { get; set; }
        public ParameterDirection Direction { get; set; }
        public bool IsNullable { get; set; }
        public string ParameterName { get; set; }
        public string SourceColumn { get; set; }
        public bool SourceColumnNullMapping { get; set; }
        public int Size { get; set; }
        public byte Scale { get; set; }
        public byte Precision { get; set; }
        public string SystemName { get; set; }
        public string DefaultValue { get; set; }
        public virtual KpiType Kpi { get; set; }
    }
}