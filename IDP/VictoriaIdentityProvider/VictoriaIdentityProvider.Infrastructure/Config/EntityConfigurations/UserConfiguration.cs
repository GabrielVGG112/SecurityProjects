using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Models;
using VictoriaIdentityProvider.Infrastructure.Config.Converters;

namespace VictoriaIdentityProvider.Infrastructure.Config.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        private readonly ICipher _cipher;

        public UserConfiguration(ICipher cipher)
        {
            _cipher = cipher;
        }

        public void Configure(EntityTypeBuilder<User> builder)
        {
            var converter = new AesValueConverter(_cipher);

            builder.ToTable("USERS");
            builder.HasKey(x => x.Id);

            // Unique index on Email
            builder
                .HasIndex(u => u.Email)
                .IsUnique();

            builder.HasMany(u => u.RefreshTokens).WithOne(u => u.User);
            builder.HasMany(u => u.UserSessions).WithOne(u => u.User);
            // Encrypted properties
            builder
                .Property(u => u.Email)
                .IsRequired().HasMaxLength(256)
                .HasConversion(converter);

            builder
                .Property(u => u.FirstName).HasMaxLength(100)
                .IsRequired()
                .HasConversion(converter);

            builder.Property(u => u.UserStatus)
                    .HasConversion(new EnumConverter<UserStatusEnum>())
                    .IsRequired();

            builder
                .Property(u => u.LastName)
                .HasMaxLength(100)
                .IsRequired()
                .HasConversion(converter);

            builder.Property(u => u.Password).HasMaxLength(512).IsRequired();

            // Shadow properties for auditing
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
