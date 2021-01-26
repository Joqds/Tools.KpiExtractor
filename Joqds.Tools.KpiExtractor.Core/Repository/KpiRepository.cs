using Joqds.Tools.KpiExtractor.Contract;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Joqds.Tools.KpiExtractor.Core.Repository
{
    public class KpiRepository<TDbContext, TCalendar> : KpiRepository<KpiValue, TDbContext,TCalendar> where TDbContext : DbContext,IKpiEnableDbContext where TCalendar :Calendar,new()
    {

        public KpiRepository(KpiDbContext context, TDbContext targetDbContext) : base(context, targetDbContext)
        {
            
        }

        protected override async Task<Tuple<TimeBucket<TCalendar>, KpiValue>> Extract(KpiType kpi, TimeBucket<TCalendar> timeBucket, string targetId)
        {
            var parameters = SetParametersValue(kpi.Id, timeBucket, targetId, kpi.Parameters).Cast<object>().ToArray();
            var commandText = new SqlCommand(kpi.Query).CommandText;




            var kpiValue = _targetDbContext.Set<KpiValue>().FromSqlRaw(commandText, parameters).AsNoTracking()
                               .FirstOrDefault() ?? new KpiValue()
                           {
                               Value = 0,
                               DateTimePoint = timeBucket.Start,
                               KpiTypeId = kpi.Id,
                               ExtractedDateTime = DateTime.Now,
                               TargetId = targetId,
                           };
            if (!timeBucket.IsPartial)
            {
                await Context.KpiValues.AddAsync(kpiValue);
                await Context.SaveChangesAsync();
            }

            return new Tuple<TimeBucket<TCalendar>, KpiValue>(timeBucket, kpiValue);
        }

        public override async Task<IEnumerable<Tuple<TimeBucket<TCalendar>, KpiValue>>> GetIfExist(KpiType kpi, IEnumerable<TimeBucket<TCalendar>> timeBuckets, string targetId)
        {
            var startPoints = timeBuckets.Where(x=>!x.IsPartial).Select(x => x.Start).ToList();
            var kpiValues = await Context.KpiValues
                .Where(x => x.KpiTypeId == kpi.Id && startPoints.Contains(x.DateTimePoint)).ToListAsync();
            var keyValuePairs = timeBuckets.Select(x => new Tuple<TimeBucket<TCalendar>, KpiValue>(x, kpiValues.FirstOrDefault(y => y.DateTimePoint == x.Start)))
                    .ToList();
            return keyValuePairs;
        }
    }

    public class CreateKpiDto<T>
    {
        public IQueryable<T> Query { get; set; }
        public int? CleanUpAfter { get; set; }
        public DatePart DatePart { get; set; }
        public ExtractionType ExtractionType { get; set; }
    }

    public abstract class KpiRepository<T, TDbContext, TCalendar> where T : class where TDbContext : IKpiEnableDbContext<T> where TCalendar:Calendar, new()
    {
        public readonly KpiDbContext Context;
        protected readonly TDbContext _targetDbContext;

        private const string TargetIdName = "targetId";
        private const string StartName = "start";
        private const string EndName = "end";
        private const string KpiIdName = "kpiId";
        private readonly PersianCalendar _persianCalendar = new PersianCalendar();

        protected KpiRepository(KpiDbContext context, TDbContext targetDbContext)
        {
            Context = context;
            _targetDbContext = targetDbContext;
        }

        public async Task<IEnumerable<Tuple<TimeBucket<TCalendar>, T>>> GetValues(int kpiTypeId, IEnumerable<TimeBucket<TCalendar>> timeBuckets, string targetId)
        {
            var kpi = await Context.KpiTypes.Include(x => x.Parameters).FirstOrDefaultAsync(x => x.Id == kpiTypeId);
            if (kpi == null)
            {
                throw new ArgumentOutOfRangeException(nameof(kpiTypeId));
            }

            var result = await GetIfExist(kpi, timeBuckets, targetId);
            if (kpi.ExtractionType == ExtractionType.OnRequest)
            {
                result = await ExtractMissingValue(result, kpi, targetId);
            }

            return result;
        }

        //todo: fill on schedule


        public async Task<IEnumerable<SqlParameter>> GetParameterList(int kpiTypeId)
        {
            var kpiType = await Context.KpiTypes.Include(x => x.Parameters).FirstOrDefaultAsync(x => x.Id == kpiTypeId);
            if (kpiType == null)
            {
                throw new ArgumentOutOfRangeException(nameof(kpiTypeId));
            }

            return kpiType.Parameters.Select(kpiParameter => new SqlParameter(
                kpiParameter.ParameterName,
                SqlDbType.Int,
                kpiParameter.Size,
                kpiParameter.Direction,
                kpiParameter.IsNullable,
                kpiParameter.Precision,
                kpiParameter.Scale,
                kpiParameter.SourceColumn,
                DataRowVersion.Default,
                null
            ));
        }

        public async Task SetParameterName(KpiType kpi, string targetIdName, string startName, string endName, string kpiIdName)
        {

            var targetIdParameter = kpi.Parameters.FirstOrDefault(x => x.ParameterName == targetIdName &&
                                                                       (x.Direction == ParameterDirection.Input || x.Direction == ParameterDirection.InputOutput));
            var startParameter = kpi.Parameters.FirstOrDefault(x => x.ParameterName == startName &&
                                                                    (x.Direction == ParameterDirection.Input || x.Direction == ParameterDirection.InputOutput));
            var endParameter = kpi.Parameters.FirstOrDefault(x => x.ParameterName == endName &&
                                                                  (x.Direction == ParameterDirection.Input || x.Direction == ParameterDirection.InputOutput));

            var kpiIdParameter = kpi.Parameters.FirstOrDefault(x => x.ParameterName == kpiIdName);

            if (targetIdParameter != null && startParameter != null && endParameter != null &&
                targetIdParameter.Id != startParameter.Id && startParameter.Id != endParameter.Id && kpiIdParameter != null
                && kpiIdParameter != targetIdParameter && kpiIdParameter != startParameter && kpiIdParameter != endParameter)
            {
                targetIdParameter.SystemName = TargetIdName;
                startParameter.SystemName = StartName;
                endParameter.SystemName = EndName;
                kpiIdParameter.SystemName = KpiIdName;
                kpi.Status = KpiTypeStatus.Active;
            }

            await Context.SaveChangesAsync();
        }

        public async Task<KpiType> Get(int kpiTypeId)
        {
            return await Context.KpiTypes.Include(x => x.Parameters)
                .SingleAsync(x => x.Status == KpiTypeStatus.Active && x.Id == kpiTypeId);
        }

        public async Task SetParameterName(KpiType kpi)
        {
            var targetIdParameter = kpi.Parameters.FirstOrDefault(x => x.ParameterName.Contains(TargetIdName) &&
                                                                       (x.Direction == ParameterDirection.Input || x.Direction == ParameterDirection.InputOutput)
                                                                       );
            var startParameter = kpi.Parameters.FirstOrDefault(x => x.ParameterName.Contains(StartName) &&
                                                                    (x.Direction == ParameterDirection.Input || x.Direction == ParameterDirection.InputOutput));
            var endParameter = kpi.Parameters.FirstOrDefault(x => x.ParameterName.Contains(EndName) &&
                                                                  (x.Direction == ParameterDirection.Input || x.Direction == ParameterDirection.InputOutput));

            var kpiIdParameter = kpi.Parameters.FirstOrDefault(x => x.ParameterName.Contains(KpiIdName));

            if (targetIdParameter != null && startParameter != null && endParameter != null &&
                targetIdParameter.Id != startParameter.Id && startParameter.Id != endParameter.Id && kpiIdParameter != null
                && kpiIdParameter != targetIdParameter && kpiIdParameter != startParameter && kpiIdParameter != endParameter)
            {
                targetIdParameter.SystemName = TargetIdName;
                startParameter.SystemName = StartName;
                endParameter.SystemName = EndName;
                kpiIdParameter.SystemName = KpiIdName;
                kpi.Status = KpiTypeStatus.Active;
            }

            await Context.SaveChangesAsync();
        }

        public async Task<KpiType> CreateAndSetParametersAndSave(CreateKpiDto<T> input)
        {
            var andSave = await CreateAndSave(input);
            await SetParameterName(andSave);
            return andSave;
        }
        public async Task<KpiType> CreateAndSave(CreateKpiDto<T> input)
        {
            //todo:[kpi] check if parameters is not start, end and targetId , throw exception

            var dbCommand = input.Query.CreateDbCommand();

            List<KpiParameter> kpiParameters = new List<KpiParameter>();
            foreach (DbParameter parameter in dbCommand.Parameters)
            {
                kpiParameters.Add(new KpiParameter()
                {
                    DbType = parameter.DbType.ToSqlDbType(),
                    Direction = parameter.Direction,
                    IsNullable = parameter.IsNullable,
                    ParameterName = parameter.ParameterName,
                    Size = parameter.Size,
                    SourceColumn = parameter.SourceColumn,
                    SourceColumnNullMapping = parameter.SourceColumnNullMapping,
                    Scale = parameter.Scale,
                    Precision = parameter.Precision,
                    DefaultValue = parameter.Value.ToString()
                });
            }

            return await CreateAndSave(dbCommand.CommandText, kpiParameters, input.CleanUpAfter, input.DatePart, input.ExtractionType);
        }

        public async Task<KpiType> CreateAndSave(string commandText, ICollection<KpiParameter> kpiParameters, int? cleanUpAfter, DatePart datePart,
            ExtractionType extractionType)
        {
            var kpi = new KpiType()
            {
                Query = commandText,
                Parameters = kpiParameters,
                CleanUpAfter = cleanUpAfter,
                DatePart = datePart,
                ExtractionType = extractionType,
                Status = KpiTypeStatus.Draft
            };
            await Context.KpiTypes.AddAsync(kpi);

            await Context.SaveChangesAsync();
            return kpi;
        }

        protected List<SqlParameter> SetParametersValue(int kpiTypeId, TimeBucket<TCalendar> timeBucket, string targetId, IEnumerable<KpiParameter> kpiParameters)
        {
            var targetParameter = GetSqlParameter(kpiParameters.First(x => x.SystemName == TargetIdName), targetId);
            var startParameter = GetSqlParameter(kpiParameters.First(x => x.SystemName == StartName), timeBucket.Start);
            var endParameter = GetSqlParameter(kpiParameters.First(x => x.SystemName == EndName), timeBucket.End);
            var kpiIdParameter = GetSqlParameter(kpiParameters.First(x => x.SystemName == KpiIdName), kpiTypeId);
            var result = new List<SqlParameter>() { targetParameter, startParameter, endParameter, kpiIdParameter };
            foreach (var kpiParameter in kpiParameters.Where(x => x.SystemName == null))
            {
                result.Add(GetSqlParameter(kpiParameter, kpiParameter.DefaultValue));
            }
            return result;
        }

        private SqlParameter GetSqlParameter(KpiParameter kpiParameter, object value)
        {
            return new SqlParameter(kpiParameter.ParameterName,
                kpiParameter.DbType,
                kpiParameter.Size,
                kpiParameter.Direction,
                kpiParameter.IsNullable,
                kpiParameter.Precision,
                kpiParameter.Scale,
                kpiParameter.SourceColumn,
                DataRowVersion.Default,
                value);
        }

        protected virtual async Task<List<Tuple<TimeBucket<TCalendar>, T>>> ExtractMissingValue(
            IEnumerable<Tuple<TimeBucket<TCalendar>, T>> values, KpiType kpi, string targetId)
        {
            // var sqlCommand = new SqlCommand(kpi.Query);
            var items = values.Where(x => x.Item2 == null).ToList();
            var items2 = values.Where(x => x.Item2 != null).ToList();
            for (var i = 0; i < items.Count; i++)
            {
                var (itemI, _) = items[i];
                items[i] = await Extract(kpi, itemI, targetId);
            }

            await Context.SaveChangesAsync();

            return items.Concat(items2).OrderBy(x => x.Item1).ToList();
        }

        protected abstract  Task<Tuple<TimeBucket<TCalendar>, T>> Extract(KpiType kpi, TimeBucket<TCalendar> timeBucket,
            string targetId);

        public abstract Task<IEnumerable<Tuple<TimeBucket<TCalendar>, T>>> GetIfExist(KpiType kpiType, IEnumerable<TimeBucket<TCalendar>> timeBuckets,  string targetId);

    }
}
