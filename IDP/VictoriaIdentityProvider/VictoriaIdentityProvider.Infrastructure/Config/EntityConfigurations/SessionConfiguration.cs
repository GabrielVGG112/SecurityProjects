using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Models;
using VictoriaIdentityProvider.Infrastructure.Config.Converters;

namespace VictoriaIdentityProvider.Infrastructure.Config.EntityConfigurations
{
    public class SessionConfiguration : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> builder)
        {
            builder.ToTable("USER_SESSION");
            builder
                .HasOne(u=> u.User)
                .WithMany(u=>u.UserSessions)
                .HasForeignKey(u=>u.UserId).OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(s => s.RefreshTokens)
                .WithOne(s => s.UserSession).HasForeignKey(rt=> rt.UserSessionId);


            builder.Property(s => s.UserAgent).IsRequired();
            builder.Property(s => s.IpAddress).IsRequired();
            builder.Property(s => s.SessionToken).IsRequired();
            builder.Property(s => s.Location).IsRequired(false);
            builder.Property(s => s.DeviceName).IsRequired(false);
            builder.Property(s => s.DeviceId).IsRequired(false);


            builder.Property(r => r.RevokedReason).HasConversion(new EnumConverter<RevokedReasonEnum>());
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
