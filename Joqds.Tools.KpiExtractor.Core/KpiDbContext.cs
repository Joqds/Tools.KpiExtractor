using Joqds.Tools.KpiExtractor.Contract;
using Microsoft.EntityFrameworkCore;

namespace Joqds.Tools.KpiExtractor.Core
{
   public class KpiDbContext:DbContext
   {
      public KpiDbContext(DbContextOptions<KpiDbContext> options):base(options)
      {
      }
      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         base.OnModelCreating(modelBuilder);

         modelBuilder.Entity<KpiType>()
            .HasMany(x => x.KpiValues)
            .WithOne(x => x.KpiType)
            .HasForeignKey(x => x.KpiTypeId);


         modelBuilder.Entity<KpiType>()
            .HasMany(x => x.Parameters)
            .WithOne(x => x.Kpi)
            .HasForeignKey(x => x.KpiId);
         
      }

      protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
      {
         base.OnConfiguring(optionsBuilder);
         if (!optionsBuilder.IsConfigured)
         {
            optionsBuilder.UseSqlServer(
               "Server=94.182.181.229;Database=SeptaPay;User Id=septapaydba;Password=aA123456@;MultipleActiveResultSets=true");
         }
      }

      public virtual DbSet<KpiValue> KpiValues { get; set; }
      public virtual DbSet<KpiType> KpiTypes { get; set; }
      public virtual DbSet<KpiParameter> KpiParameters { get; set; }


   }
}
