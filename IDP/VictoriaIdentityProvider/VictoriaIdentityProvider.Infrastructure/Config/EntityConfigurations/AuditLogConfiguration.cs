using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Infrastructure.Config.EntityConfigurations
    {
        public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
        {
            public void Configure(EntityTypeBuilder<AuditLog> builder)
            {
                builder.ToTable("AUDIT_LOGS");

                builder.HasKey(a => a.Id);

         
                builder
                    .HasOne(a => a.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.Property(a => a.EventType)
                    .IsRequired()
                    .HasMaxLength(100);

                builder.Property(a => a.Message)
                    .IsRequired()
                    .HasMaxLength(1000);

                builder.Property(a => a.CreatedAtUtc)
                    .IsRequired();

              
                builder.Property(a => a.Ip)
                    .HasMaxLength(64);

                builder.Property(a => a.UserAgent)
                    .HasMaxLength(512);

                builder.Property(a => a.SessionId);

                builder.Property(a => a.TokenId);


            builder
    .Property<byte[]>("Version")
    .IsRowVersion()
    .ValueGeneratedOnAddOrUpdate();

            builder
                .Property<DateTime>("CreatedAt")
                .HasDefaultValueSql("GETUTCDATE()");

            builder
                .Property<DateTime>("UpdatedAt")
                .HasDefaultValueSql("GETUTCDATE()");
        }
        }
    }

