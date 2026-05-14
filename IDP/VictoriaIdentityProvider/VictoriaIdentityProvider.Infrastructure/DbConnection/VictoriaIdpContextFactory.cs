using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using VictoriaIdentityProvider.Infrastructure.Config.Interceptors;
using VictoriaIdentityProvider.Infrastructure.Security;

namespace VictoriaIdentityProvider.Infrastructure.DbConnection
{
    public class VictoriaIdpDbContextFactory : IDesignTimeDbContextFactory<VictoriaIdpDbContext>
    {
        public VictoriaIdpDbContext CreateDbContext(string[] args)
        {
            // Load configuration (appsettings.json)
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets<VictoriaIdpDbContextFactory>()
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<VictoriaIdpDbContext>();

            var connectionString = config.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);

         
            
            var hasher = new AesHasher(config);
            var interceptor = new AuditInterceptor();
            return new VictoriaIdpDbContext(optionsBuilder.Options, hasher,interceptor);
        }
    }
}
