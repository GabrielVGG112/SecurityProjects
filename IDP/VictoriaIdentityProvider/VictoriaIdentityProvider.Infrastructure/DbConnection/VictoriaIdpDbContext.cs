using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;
using VictoriaIdentityProvider.Domain.Models;
using VictoriaIdentityProvider.Infrastructure.Config.EntityConfigurations;
using VictoriaIdentityProvider.Infrastructure.Config.Interceptors;


namespace VictoriaIdentityProvider.Infrastructure.DbConnection
{
    public class VictoriaIdpDbContext : DbContext
    {
        private readonly ICipher _hasher;
        private readonly AuditInterceptor _interceptor;

        public DbSet<User> Users { get; set; }
        
        public DbSet <UserSession> Sessions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public VictoriaIdpDbContext(DbContextOptions<VictoriaIdpDbContext> options, ICipher hasher,AuditInterceptor interceptor) : base (options)
        {
            _hasher = hasher;
            _interceptor = interceptor;
        }

        public VictoriaIdpDbContext(DbContextOptions<VictoriaIdpDbContext> options) : base (options)
        {
          
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_interceptor);
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (_hasher is not null)
            {
                modelBuilder.ApplyConfiguration(new UserConfiguration(_hasher));
                // Here with constructor parameters
            }

          
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(),t=>
            t.GetConstructors().Any(c=> c.GetParameters().Length == 0));
            base.OnModelCreating(modelBuilder);
            // any with 0 contructor parameters
        }
    }
}
