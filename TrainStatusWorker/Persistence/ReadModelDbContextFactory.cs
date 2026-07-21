using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace TrainStatusWorker.Persistence
{
    public class ReadModelDbContextFactory : IDesignTimeDbContextFactory<ReadModelDbContext>
    {
        public ReadModelDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("ReadModelDb");

            var optionsBuilder = new DbContextOptionsBuilder<ReadModelDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ReadModelDbContext(optionsBuilder.Options); 
        }
    }
}
