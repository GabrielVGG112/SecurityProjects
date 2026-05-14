using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VictoriaIdentityProvider.Application.Configuration;
using VictoriaIdentityProvider.Application.EventBus;
using VictoriaIdentityProvider.Application.EventBus.Interfaces;
using VictoriaIdentityProvider.Application.Events;
using VictoriaIdentityProvider.Application.Interfaces;
using VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.OptionsInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.OrchestrationInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.ValidatorInterfaces;
using VictoriaIdentityProvider.Application.Services.ClientServices;
using VictoriaIdentityProvider.Application.Services.Factory;
using VictoriaIdentityProvider.Application.Services.Factory.RepositoryFactory;
using VictoriaIdentityProvider.Application.Services.OrchestrationServices;
using VictoriaIdentityProvider.Application.Services.TokenServices;
using VictoriaIdentityProvider.Application.Validators;
using VictoriaIdentityProvider.Application.Validators.HeaderValidation;
using VictoriaIdentityProvider.Application.Validators.InputValidation;
using VictoriaIdentityProvider.Domain.Interfaces;
using VictoriaIdentityProvider.Domain.Models;
using VictoriaIdentityProvider.Infrastructure.Config.Interceptors;
using VictoriaIdentityProvider.Infrastructure.Config.OptionsModels;
using VictoriaIdentityProvider.Infrastructure.DbConnection;
using VictoriaIdentityProvider.Infrastructure.EventHandlers;
using VictoriaIdentityProvider.Infrastructure.Repositories;
using VictoriaIdentityProvider.Infrastructure.Security;
namespace VictoriaIdentityProvider.Api.Startup;

public static class ServiceConfigurations
{
    public static void AddPersonalOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Argon2Options>(configuration.GetSection("Security:Argon2Options"));
        services.Configure<SecurityOptions>(
            o =>
            {
                o.PasswordRequireDigit = true;
                o.PasswordRequireLowercase = true;
                o.PasswordRequireUppercase = true;
                o.CheckBreachedPasswords = true;
                o.PasswordMinLength = 8;
                o.PasswordMaxLength = 128;
                o.MaxLoginAttempts = 3;
                o.RequireEmailVerification = false;
                o.LockoutDurationMinutes = 5;
                o.EmailVerificationTokenExpirationHours = 24;
                o.PasswordResetTokenExpirationHours = 24;

            }
            );
        services.Configure<JwtKeysOptions>(configuration.GetSection("Security:Jwt"));
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));

    }




    public static void AddLifeTimeServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddDbContext<VictoriaIdpDbContext>(o => o.UseSqlServer(config.GetConnectionString("DefaultConnection")));
        // infrastructure layer
        services.AddSingleton<AuditInterceptor>();
        services.AddKeyedSingleton<IHasher, Argon2Hasher>("argon");
        services.AddKeyedSingleton<IHasher, HmacSha256Hasher>("hmac");

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ICipher, AesHasher>();
        // app layer - Factories
        services.AddScoped<SessionFactory>();
        services.AddScoped<TokenFactory>();
        services.AddScoped<IModelFactory<UserSession>, SessionFactory>();
        services.AddScoped<IModelFactory<RefreshToken>, TokenFactory>();
        services.AddScoped<ITokenFactory, TokenFactory>();


        // app layer - Security & Settings
        services.AddSingleton<IUserSecuritySettings>(sp =>
        new UserSecuritySettings(sp.GetRequiredService<IOptions<SecurityOptions>>())
        );
        services.AddSingleton<IJwtSecuritySettings>(sp =>
         new JwtSecuritySettings(sp.GetRequiredService<IOptions<SecurityOptions>>())
        );


        // app layer - Validators
        services.AddScoped<HeaderValidatorFacade>();
        services.AddScoped<FieldValidatorFacade>();
        services.AddScoped<SessionValidator>();
        services.AddScoped<JwtTokenValidator>();
        services.AddScoped<RefreshTokenValidator>();


        services.AddScoped<EmailValidator>();
        services.AddScoped<PasswordValidator>();
        services.AddScoped<NameValidator>();
        services.AddScoped<PhoneNumberValidator>();





        services.AddScoped<IAsyncValidator<string>, JwtTokenValidator>();
        services.AddScoped<IAsyncValidator<string>, RefreshTokenValidator>();
        services.AddScoped<IAsyncValidator<string>, SessionValidator>();

        services.AddScoped<IAsyncValidator<string>, PasswordValidator>();
        services.AddScoped<IValidator<string>, EmailValidator>();
        services.AddScoped<IValidator<string>, PhoneNumberValidator>();
        services.AddScoped<IValidator<string>, NameValidator>();

        // orchestration
        services.AddScoped<ModelFactoryFacade>();
        services.AddScoped<AuthFacade>();

        services.AddScoped<IRegistrationManager, RegistrationManager>();
        services.AddScoped<IRepositoryFactory, RepositoryFactory>();
        services.AddScoped<ITokenRotationService, TokenRotationService>();
        // app layer - Client Services
        services.AddScoped<IClientContextService, ClientContextService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<ILogoutService, LogoutService>();

        // app layer - Token Services

        services.AddScoped<IJwtInternalToken, JwtInternalTokenService>();
        services.AddScoped<IJwtAccessTokenService, JwtAccessTokenService>();
        services.AddScoped<IDefaultTokenService, DefaultTokenService>();


        services.AddHttpClient<PasswordValidator>(client =>
        {
            client.BaseAddress = new Uri("https://api.pwnedpasswords.com/");
            client.DefaultRequestHeaders.Add("User-Agent", "VictoriaIdentityProvider/1.0");
            client.Timeout = TimeSpan.FromSeconds(5);
        });
    }



    public static void AddAuditlogs(this IServiceCollection services)
    {
        services.AddScoped<IEventDispatcher, EventDispatcher>();
        services.AddScoped<IEventHandler<LoginEvent>,LoginAuditEvent>();
    }
}

