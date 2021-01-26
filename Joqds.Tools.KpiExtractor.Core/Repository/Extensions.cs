using Joqds.Tools.KpiExtractor.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Joqds.Tools.KpiExtractor.Core.Repository
{
    public static class Extensions
    {
        public static IServiceCollection RegisterKpiExtractor<TDbContext, TKpiSeeder,TCalendar>(this IServiceCollection services) where TDbContext : DbContext, IKpiEnableDbContext where TKpiSeeder : class, IKpiSeeder where TCalendar : Calendar, new()
        {
            return services.AddScoped<IKpiEnableDbContext, TDbContext>()
            .AddScoped<IKpiSeeder, TKpiSeeder>()
            .AddScoped<KpiRepository<TDbContext,TCalendar>>();
        }
    }
}