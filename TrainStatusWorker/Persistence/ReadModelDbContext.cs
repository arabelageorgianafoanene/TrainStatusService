using Microsoft.EntityFrameworkCore;
using TrainStatusWorker.ReadModels;

namespace TrainStatusWorker.Persistence
{
    public class ReadModelDbContext : DbContext
    {
        public DbSet<TrainSummary> TrainSummaries => Set<TrainSummary>();

        public ReadModelDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<ReadModelDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrainSummary>().HasKey(ts => ts.TrainId);
        }
    }
}
