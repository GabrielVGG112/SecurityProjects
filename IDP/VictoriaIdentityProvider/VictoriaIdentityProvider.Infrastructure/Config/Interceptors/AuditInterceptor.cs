using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace VictoriaIdentityProvider.Infrastructure.Config.Interceptors
{
    public  class AuditInterceptor : SaveChangesInterceptor
    {

        public override InterceptionResult<int> SavingChanges(
      DbContextEventData eventData,
      InterceptionResult<int> result)
        {

            var context = eventData.Context ??
                throw new InvalidOperationException("Something  went wrong ");    
           
            foreach(var entry in context.ChangeTracker.Entries()) 
            {
                if (!entry.Metadata.GetProperties().Any(p => p.Name == "CreatedAt" || p.Name == "UpdatedAt"))
                    continue;

                if (entry.State == EntityState.Added) 
                {
                    entry.Property("CreatedAt").CurrentValue =DateTime.UtcNow;
                    entry.Property("UpdatedAt").CurrentValue =DateTime.UtcNow;

                }
                if (entry.State == EntityState.Modified && entry.Properties.Any(p => p.IsModified))
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context ??
                throw new InvalidOperationException("Something  went wrong ");

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (!entry.Metadata.GetProperties().Any(p => p.Name == "CreatedAt" || p.Name == "UpdatedAt"))
                    continue;

                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;

                }
                if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
