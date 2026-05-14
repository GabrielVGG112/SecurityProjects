using Microsoft.Extensions.DependencyInjection;
using VictoriaIdentityProvider.Application.DTOs;
using VictoriaIdentityProvider.Application.Interfaces.ClientInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.OptionsInterfaces;
using VictoriaIdentityProvider.Application.Interfaces.SecurityInterfaces;
using VictoriaIdentityProvider.Application.Services.OrchestrationServices;
using VictoriaIdentityProvider.Domain.CustomErrors;
using VictoriaIdentityProvider.Domain.Enums;
using VictoriaIdentityProvider.Domain.Interfaces;
using VictoriaIdentityProvider.Domain.Models;

namespace VictoriaIdentityProvider.Application.Services.ClientServices
{
    public class UserService :IUserService
    {
       private readonly FieldValidatorFacade _validator;
       private readonly  IHasher _passwordHasher;
        private readonly IUserRepository _repo;
        private readonly IUserSecuritySettings _settings;
        public UserService(
            FieldValidatorFacade validator,
            IUserRepository repo,IUserSecuritySettings settings,
            [FromKeyedServices("argon")] IHasher hasher 

            )
        {
            _passwordHasher = hasher;
            _validator = validator;
            _repo = repo;
            _settings = settings;
         
        }
        public async Task<User> GetUserByEmailAddressAsync (string emailAddress) 
        {
            var result = _validator
                .ValidateRegistration(Enums.ValidationField.Email, emailAddress);

            if (!result.IsValid) throw new ValidationError(result.Errors);
            var user = await _repo.GetUserByEmailAsync(emailAddress) 
                    ?? throw new UserException("User with this email does not exist");

            return user;
        }
        public async Task<User> CreateUserAsync(RegisterDto registerDto) 
        {
            if (registerDto.Password != registerDto.ConfirmPassword)
                throw new ValidationError(["Passwords do not match"]);

            var result =  await _validator.ValidateRegistration(registerDto);

            if (!result.IsValid)
                throw new ValidationError(result.Errors);
            
            var isAlredy = await _repo.VerifyIfEmailExistsAsync(registerDto.EmailAddress);
            
            if (isAlredy) 
                throw new UserException("An user with same e-mail address exists  allredy");


            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.EmailAddress,
                Password = _passwordHasher.HashData(registerDto.Password),
                UserStatus = UserStatusEnum.PendingVerification,
                IsEmailConfirmed = false,
               
                LockoutEnd = null,
                LastLogInAt = null,
                FailedLoginAttempts = 0
            };


            await _repo.AddUserAsync(newUser);

            return newUser;
        }


        public async Task<User> GetUserByIdAsync(Guid id)  
        {
            if (id == Guid.Empty) 
                throw new UserException("An unique identitfier is mandatory to find the user");

          var user = await   _repo.GetUserByIdAsync(id) 
                ?? throw new UserException("An user with this id doesnt exist");
            return user;
        }
        public async Task ValidatePassword(User user , string password) 
        {
            var result = await _validator.ValidateRegistrationAsync(Enums.ValidationField.Password, password);
            if (!result.IsValid) 
            {
                throw new ValidationError(result.Errors);
            }

            if (!_passwordHasher.VerifyData(password,user.Password)) 
                throw new ValidationError(["Invalid password"]);
        }


        public async Task MakeUserLockout(User user) 
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts % _settings.MaxLoginAttempts <= 0)
            {
                var x = user.FailedLoginAttempts / _settings.MaxLoginAttempts;
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(_settings.LockoutDurationMinutes * x);

            }
            await  _repo.UpdateUserAsync(user);
        }


        public void ValidateLockout(User user)
        {
            if (user.IsLocked && user.LockoutEnd.HasValue)
                throw new LockoutExpirationException
                    ($"Account locked until {user.LockoutEnd.Value:yyyy-MM-dd HH:mm:ss} UTC");
        }


        public void ValidateEmailConfirmation (User user) 
        {
          
            if (!user.IsEmailConfirmed && _settings.RequireEmailVerification)
                throw new EmailNotVerifiedException("You have to confirm your email");
        }
        public async Task UpdateUserAsync(User user) 
        {
            bool isAlredy = await _repo.VerifyIfIdExistsAsync(user.Id);

            if (!isAlredy) throw new UserException("User Does not exist");

            await _repo.UpdateUserAsync(user);
        }

    }

}
