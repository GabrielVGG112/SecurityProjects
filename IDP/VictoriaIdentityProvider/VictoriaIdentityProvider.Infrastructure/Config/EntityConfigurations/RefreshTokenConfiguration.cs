using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Models;
using VictoriaIdentityProvider.Infrastructure.Config.Converters;

namespace VictoriaIdentityProvider.Infrastructure.Config.EntityConfigurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("REFRESH_TOKENS");

            builder.HasOne(u => u.User).WithMany(t => t.RefreshTokens).HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict); ;
          
            builder.HasOne(r => r.UserSession).WithMany(s => s.RefreshTokens);


            builder.Property(r => r.TokenHash).IsRequired();
         

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
